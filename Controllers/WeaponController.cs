using System.Threading.Tasks;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Service.WeaponService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")] //find specific controller controller can be access by name
    public class WeaponController:ControllerBase
    {
        readonly IWeaponService _weaponService;

        public WeaponController(IWeaponService weaponService)
        {
            _weaponService = weaponService;
        }
        
        [HttpPost]
        public async Task<IActionResult> AddWeapon(AddWeaponDto newWeaponDto)
        {
            return Ok(await _weaponService.AddWeapon(newWeaponDto));
        }
        [HttpPut]
        public async Task<IActionResult> UpdateWeapon(AddWeaponDto update)
        {
            return Ok(await _weaponService.UpdateWeapon(update));
        }

    }
}