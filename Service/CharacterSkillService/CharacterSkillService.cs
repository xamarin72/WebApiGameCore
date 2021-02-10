using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.CharacterSkil;
using dotnet_rpg.Migrations;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Service.CharacterSkillService
{
    public class CharacterSkillService:ICharacterSkillService
    {
        readonly DataContext _dataContext;
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly IMapper _mapper;

        public CharacterSkillService(DataContext dataContext, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _dataContext = dataContext;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.CharacterSkills).ThenInclude(c => c.Skill)
                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId
                                                              && c.User.Id == int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)));
                if (character==null)
                {
                    response.Success = false;
                    response.Message = "character not found";
                    return response;
                }

                Skill skill = await _dataContext.Skills.FirstOrDefaultAsync(c => c.Id == newCharacterSkill.SkillId);
                if (skill==null)
                {
                    response.Success = false;
                    response.Message = "skill not found";
                    return response;
                }

                CharacterSkill characterSkill = new CharacterSkill
                {
                    Character = character,
                    Skill = skill
                };

                await _dataContext.CharacterSkills.AddAsync(characterSkill);
                await _dataContext.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);
                
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
                
            }

            return response;
        }
    }
}