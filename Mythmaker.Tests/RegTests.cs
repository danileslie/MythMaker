using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MythMaker.Models;
using Xunit;

namespace MythMaker.Tests
{
    public class RegTests
    {
        [Fact]
        public void Register_ValidData_PassesValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "SomePassword123!",
                ConfirmPassword = "SomePassword123!"
            };
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void Register_MismatchedPasswords_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "SomePassword123!",
                ConfirmPassword = "SomePassword124!"
            };
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

            // Assert
            Assert.False(isValid);
        }
    }
}