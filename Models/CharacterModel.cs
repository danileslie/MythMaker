using System.ComponentModel.DataAnnotations;

namespace MythMaker.Models
{
    public class Character
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string Race { get; set; }

        [MaxLength(50)]
        public string Class { get; set; }

        [Range(1, 20)]
        public int Level { get; set; }

        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }

        [MaxLength(10000)]
        public string Backstory { get; set; }

        public bool IsDraft { get; set; }

        [Required]
        public string OwnerId { get; set; }
    }
}