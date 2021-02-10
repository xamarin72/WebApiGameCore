using System.Linq;
using AutoMapper;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Dtos.Skill;
using dotnet_rpg.Dtos.Weapon;
using dotnet_rpg.Models;

namespace dotnet_rpg
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Character, GetCharacterDto>()
                .ForMember(c => c.Skills, c => c.MapFrom(c => c.CharacterSkills.Select(c => c.Skill)));
            
            CreateMap<AddCharacterDto, Character>();
            //there a re 2 entries
            CreateMap<AddWeaponDto, Weapon>();
            CreateMap<Weapon, AddWeaponDto>();
            
            CreateMap<Weapon, GetWeaponDto>();
            CreateMap<GetWeaponDto, Weapon>();

            CreateMap<Skill,GetSkillDto>();
            CreateMap<Character, HighScoreDto>();
            
        }
    }
}