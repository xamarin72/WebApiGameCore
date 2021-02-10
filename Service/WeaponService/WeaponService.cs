using System;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations.Rules;

namespace dotnet_rpg.Service.WeaponService
{
    public class WeaponService : IWeaponService
    {
        readonly DataContext _dataContext;
        readonly IHttpContextAccessor _contextAccessor;
        readonly IMapper _mapper;
        private int GetUserId() => int.Parse(_contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public WeaponService(DataContext dataContext, IHttpContextAccessor contextAccessor, IMapper mapper)
        {
            _dataContext = dataContext;
            _contextAccessor = contextAccessor;
            _mapper = mapper; //
        }

        async public Task<ServiceResponse<GetCharacterDto>> AddWeapon(AddWeaponDto newWeapon)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _dataContext.Characters. //.Include(c => c.User).
                    FirstOrDefaultAsync(c => c.User.Id == GetUserId() && c.Id == newWeapon.CharacterId);
                if (character == null)
                {
                    response.Success = false;
                    response.Message = "Character not found";
                    response.Data = _mapper.Map<GetCharacterDto>(character);

                    return response;
                }

                // Weapon weapon = _mapper.Map<Weapon>(newWeapon);//throw error
                Weapon weapon = new Weapon
                {
                    Name = newWeapon.Name,
                    Damage = newWeapon.Damage,
                    Character = character
                };
                await _dataContext.Weapons.AddAsync(weapon);
                await _dataContext.SaveChangesAsync();
                response.Success = true;
                response.Message = "Weapon added succesfuly.";
                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateWeapon(AddWeaponDto update)
        {
            ServiceResponse<GetCharacterDto> response = new ServiceResponse<GetCharacterDto>();
            try
            {
                Character character = await _dataContext.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.User.Id == GetUserId() && c.Id == update.CharacterId);

                if (character == null || character.Weapon==null)
                {
                    response.Success = false;
                    response.Message = "Character not found";

                    return response;
                }


              character.Weapon.Name = update.Name;
              character.Weapon.Damage = update.Damage;

                _dataContext.Characters.Update(character);
                await _dataContext.SaveChangesAsync();
                
                response.Success = true;
                response.Message = "Weapon updated succesfuly.";
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