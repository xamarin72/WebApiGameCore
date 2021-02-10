using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace dotnet_rpg.Service.FightService
{
    public class FightService : IFightService
    {
        readonly DataContext _dataContext;
        readonly IMapper _mapper;

        public FightService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
        {
            ServiceResponse<AttackResultDto> serviceResponse = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                Character opponent = await _dataContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                var damage = DoWeaponAttack(attacker, opponent);

                if (opponent.HitPoints < 0)
                {
                    serviceResponse.Message = string.Format("{0} has been defeated.", opponent.Name);
                }

                _dataContext.Characters.Update(opponent);
                await _dataContext.SaveChangesAsync();

                serviceResponse.Data = new AttackResultDto
                {
                    Attacker = attacker.Name, AttackerHp = attacker.HitPoints, Opponent = opponent.Name, OpponentHP = opponent.HitPoints, Damage = damage
                };
            }
            catch (Exception e)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = e.Message;
            }

            return serviceResponse;
        }

        static int DoWeaponAttack(Character attacker, Character opponent)
        {
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= new Random().Next(opponent.Defense);
            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
        {
            ServiceResponse<AttackResultDto> response = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await _dataContext.Characters
                    .Include(c => c.CharacterSkills).ThenInclude(c => c.Skill)
                    .FirstOrDefaultAsync(c => c.Id == request.AttackerId);
                Character opponent = await _dataContext.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.OpponentId);

                CharacterSkill characterSkill = attacker.CharacterSkills.FirstOrDefault(c => c.Skill.Id == request.SkillId);
                if (characterSkill == null)
                {
                    response.Success = false;
                    response.Message = string.Format("{0}  doesnt know that skill.", attacker.Name);
                    return response;
                }

                var damage = DoSkillAttack(characterSkill, attacker, opponent);

                if (opponent.HitPoints < 0)
                {
                    response.Message = string.Format("{0} has been defeated.", opponent.Name);
                }

                _dataContext.Characters.Update(opponent);
                await _dataContext.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name, AttackerHp = attacker.HitPoints, Opponent = opponent.Name, OpponentHP = opponent.HitPoints, Damage = damage
                };
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        static int DoSkillAttack(CharacterSkill characterSkill, Character attacker, Character opponent)
        {
            int damage = characterSkill.Skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= new Random().Next(opponent.Defense);
            if (damage > 0)
            {
                opponent.HitPoints -= damage;
            }

            return damage;
        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto request)
        {
            ServiceResponse<FightResultDto> response = new ServiceResponse<FightResultDto>
            {
                Data = new FightResultDto(),
            };
            try
            {
                List<Character> characters =
                    await _dataContext.Characters
                        .Include(c => c.Weapon)
                        .Include(c => c.CharacterSkills).ThenInclude(c => c.Skill)
                        .Where(c => request.CharacterIds.Contains(c.Id)).ToListAsync();

                bool defeated = false;
                while (!defeated)
                {
                    foreach (Character attacker in characters)
                    {
                        List<Character> opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        Character opponent = opponents[new Random().Next(opponents.Count)];

                        int damage = 0;
                        string attackUsed = string.Empty;

                        bool useWeapon = new Random().Next(2) == 0;
                        if (useWeapon)
                        {
                            if (attacker.Weapon != null)
                            {
                                attackUsed = attacker.Weapon.Name;
                                damage = DoWeaponAttack(attacker, opponent);
                            }
                        }
                        else
                        {
                            int totalSkills = attacker.CharacterSkills.Count;
                            if (totalSkills > 0)
                            {
                                int randomSkill = new Random().Next(totalSkills);
                                attackUsed = attacker.CharacterSkills[randomSkill].Skill.Name;
                                damage = DoSkillAttack(attacker.CharacterSkills[randomSkill], attacker, opponent);
                            }
                        }

                        response.Data.Log.Add(string.Format("{0} attacks {1} using {2} with  {3} damage", attacker.Name, opponent.Name, attackUsed, damage));
                        if (opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories++;
                            opponent.Defeats++;
                            response.Data.Log.Add(string.Format("{0} has been defeated", opponent.Name));
                            response.Data.Log.Add(string.Format("{0} wins with {1} HP left", attacker.Name, attacker.HitPoints));
                            break;
                        }
                    }
                }

                characters.ForEach(c =>
                {
                    c.Fights++;
                    c.HitPoints = 100;
                });
                _dataContext.Characters.UpdateRange(characters);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }


            return response;
        }

        public async Task<ServiceResponse<List<HighScoreDto>>> GetHighscore()
        {
            List<Character> characters = await _dataContext.Characters
                .Where(c => c.Fights > 0)
                .OrderByDescending(c => c.Victories)
                .ThenBy(c => c.Defeats)
                .ToListAsync();

            var response = new ServiceResponse<List<HighScoreDto>>
            {
                Data = characters.Select(c => _mapper.Map<HighScoreDto>(c)).ToList()
            };
            return response;
        }
    }
}