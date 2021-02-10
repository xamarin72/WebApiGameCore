using System.Threading.Tasks;
using dotnet_rpg.Dtos.CharacterSkil;
using dotnet_rpg.Service.CharacterSkillService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace dotnet_rpg.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CharacterSkillController:ControllerBase
    {
        readonly ICharacterSkillService _characterSkillService;

        public CharacterSkillController(ICharacterSkillService characterSkillService)
        {
            _characterSkillService = characterSkillService;
        }
        
        
        [HttpPost]
        public async Task<IActionResult> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {


            return Ok(await _characterSkillService.AddCharacterSkill(newCharacterSkill));
        }
    }
}