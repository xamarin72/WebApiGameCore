namespace dotnet_rpg.Models
{
    public class Weapon
    {
        public int Id { get; set; }
        public string Name { get; set; } = "first weapon";

        public int Damage { get; set; } = 1;
        public Character Character { get; set; }
        //fk
        //name convention, c class name aind id
        public int CharacterId { get; set; }
    }
}