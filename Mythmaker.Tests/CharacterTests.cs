using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MythMaker.Models;
using Xunit;

namespace MythMaker.Tests
{
    public class CharacterTests
    {
        [Fact]
        public void Character_ValidData_PassesValidation()
        {
            // a normal, fully valid character - baseline check that nothing's
            // over-restrictive and blocking data that should actually be fine
            var character = new Character
            {
                Name = "Aria",
                Level = 5,
                Backstory = "A short backstory.",
                OwnerId = "some-user-id"
            };
            var context = new ValidationContext(character);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(character, context, results, validateAllProperties: true);

            Assert.True(isValid);
        }

        [Fact]
        public void Character_MissingName_FailsValidation()
        {
            // Name is [Required] - this should never pass
            var character = new Character
            {
                Name = null,
                Level = 5,
                OwnerId = "some-user-id"
            };
            var context = new ValidationContext(character);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(character, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void Character_LevelAboveRange_FailsValidation()
        {
            // Level is capped at 20 per the PRD - 21 should fail
            var character = new Character
            {
                Name = "Aria",
                Level = 21,
                OwnerId = "some-user-id"
            };
            var context = new ValidationContext(character);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(character, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void Character_BackstoryTooLong_FailsValidation()
        {
            // Backstory caps at 10,000 chars - generating one char over the limit
            // instead of typing out a huge string by hand
            var character = new Character
            {
                Name = "Aria",
                Level = 5,
                Backstory = new string('a', 10001),
                OwnerId = "some-user-id"
            };
            var context = new ValidationContext(character);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(character, context, results, validateAllProperties: true);

            Assert.False(isValid);
        }
    }
}