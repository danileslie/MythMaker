using System.ComponentModel.DataAnnotations;

namespace MythMaker.Models
{
    // Id is nullable here specifically - null means "this is a brand new draft,
    // no row exists yet", a real value means "keep updating this existing one"
    public class AutosaveViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Race { get; set; }
        public string Class { get; set; }
        public int Level { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public string Backstory { get; set; }
    }
}