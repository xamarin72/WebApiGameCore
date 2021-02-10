using System.Collections.Generic;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Models;

namespace dotnet_rpg.Service.CharacterService
{
  public interface ICharacterService
  {

  Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters();
  Task<ServiceResponse<  GetCharacterDto >>GetCharaterById(int id);
  Task<ServiceResponse<List<GetCharacterDto>>>  AddCharacter(AddCharacterDto newCharacter);
  
  
  Task<ServiceResponse<GetCharacterDto>>  Update (UpdateCharacterDto updateCharacter);


  Task<ServiceResponse<List<GetCharacterDto>>> Delete( int id);
  }
}