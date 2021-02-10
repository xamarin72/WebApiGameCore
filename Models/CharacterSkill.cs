namespace dotnet_rpg.Models
{
    public class CharacterSkill//join class makes the fk for many to many relationship
    {
        public Character Character { get; set; }
        public Skill Skill { get; set; }
        public int CharacterId { get; set; }
        public int SkillId { get; set; }

    }
}