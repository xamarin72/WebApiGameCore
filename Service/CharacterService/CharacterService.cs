using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Service.CharacterService
{
    public class CharacterService : ICharacterService
    {
        readonly IMapper _mapper;
        readonly DataContext _dataContext;
        readonly IHttpContextAccessor _contextAccessor;

        public CharacterService(IMapper mapper, DataContext dataContext, IHttpContextAccessor contextAccessor)
        {
            _mapper = mapper;
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
        }

        private int GetUserId() => int.Parse(_contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
        
        private string  GetUserRole() => _contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            
            //Admins can see all characters
            List<Character> list = 
                GetUserRole().Equals("Admin") ? 
                    await  _dataContext.Characters.Include(c => c.Weapon).Include(c => c.CharacterSkills).ThenInclude(c => c.Skill).ToListAsync():
                    await _dataContext.Characters.Include(c => c.Weapon).Include(c => c.CharacterSkills).ThenInclude(c => c.Skill)
                .Where(c => c.User.Id == GetUserId())
                .ToListAsync();

            serviceResponse.Data = list.Select(s => _mapper.Map<GetCharacterDto>(s)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharaterById(int id)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            Character character = await _dataContext.Characters
                .Include(c => c.Weapon)
                .Include(c => c.CharacterSkills).ThenInclude(c => c.Skill)
                .FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
            if (character == null)
            {
                serviceResponse.Message = string.Format("User id = {0} is not allowed to retrieve this character.", GetUserId());
                serviceResponse.Success = false;
                return serviceResponse;
            }

            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
            //
            Character character = _mapper.Map<Character>(newCharacter);
            character.User = await _dataContext.Users.FirstOrDefaultAsync(c => c.Id == GetUserId());

            await _dataContext.AddAsync(character);
            await _dataContext.SaveChangesAsync();

            serviceResponse.Data = await _dataContext.Characters.Where(c => c.User.Id == GetUserId()).Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();

            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> Update(UpdateCharacterDto updateCharacter)
        {
            ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();
            //get character
            try
            {
                Character character = await _dataContext.Characters.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == updateCharacter.Id && c.User.Id == GetUserId());
                if (character != null && character.User.Id == GetUserId())
                {
                }

                if (character == null)
                {
                    serviceResponse.Message = string.Format("User id = {0} is not allowed to update this character.", GetUserId());
                    serviceResponse.Success = false;
                    return serviceResponse;
                }

                character.Name = updateCharacter.Name;
                character.HitPoints = updateCharacter.HitPoints;
                character.Class = updateCharacter.Class;
                character.Intelligence = updateCharacter.Intelligence;
                character.Defense = updateCharacter.Defense;
                character.Strength = updateCharacter.Strength;
                // character.User.Id = updateCharacter.UserId;
                //UPDATE  in dbset
                _dataContext.Characters.Update(character);
                await _dataContext.SaveChangesAsync();

                serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception e)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = e.Message;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> Delete(int id)
        {
            ServiceResponse<List<GetCharacterDto>> serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            //get character
            try
            {
                Character character = await _dataContext.Characters.FirstOrDefaultAsync(c => c.Id == id && c.User.Id == GetUserId());
                if (character == null)
                {
                    serviceResponse.Message = String.Format("Not allowed to delete the character");
                    serviceResponse.Success = false;
                    return serviceResponse;
                }

                _dataContext.Characters.Remove(character);
                await _dataContext.SaveChangesAsync();

                serviceResponse.Data = await _dataContext.Characters.Where(c => c.User.Id == GetUserId()).Select(s => _mapper.Map<GetCharacterDto>(s)).ToListAsync();
            }
            catch (Exception e)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = e.Message;
            }

            return serviceResponse;
        }
    }
}