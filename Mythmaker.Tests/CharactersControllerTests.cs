using Microsoft.EntityFrameworkCore;
using MythMaker.Data;
using MythMaker.Models;

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MythMaker.Tests
{
    public class CharactersControllerTests
    {
        // spins up a fresh, empty in-memory "database" for a single test -
        // guid means every call gets its own isolated db, no leaking data
        // between tests even if they run in a weird order or in parallel
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Character_Query_ExcludesOtherUsersCharacters()
        {
            // Arrange
            using var context = CreateContext();
            // Empty fields to handle In-Memory provider's lack of support for non-nullable reference types
            context.Characters.Add(new Character { Name = "Aria", Level = 5, Race = "", Class = "", Backstory = "", OwnerId = "user-1" });
            context.Characters.Add(new Character { Name = "Borin", Level = 3, Race = "", Class = "", Backstory = "", OwnerId = "user-2" });
            await context.SaveChangesAsync();

            // Act - same query shape as CharactersController.Index()
            var user1Characters = await context.Characters
                .Where(c => c.OwnerId == "user-1" && !c.IsDraft)
                .ToListAsync();

            // Assert - only user-1's character comes back, not user-2's
            Assert.Single(user1Characters);
            Assert.Equal("Aria", user1Characters[0].Name);
        }

        [Fact]
        public async Task Character_Create_Persists()
        {
            // Arrange
            using var context = CreateContext();
            var character = new Character { Name = "Aria", Level = 5, Race = "", Class = "", Backstory = "", OwnerId = "user-1" };

            // Act
            context.Characters.Add(character);
            await context.SaveChangesAsync();

            // Assert - re-query it back rather than trusting the in-memory object still has the right state
            var saved = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
            Assert.NotNull(saved);
            Assert.Equal("Aria", saved.Name);
        }

        [Fact]
        public async Task Character_Edit_PersistsChanges()
        {
            // Arrange - save a character first
            using var context = CreateContext();
            var character = new Character { Name = "Aria", Level = 5, Race = "Elf", Class = "Wizard", Backstory = "", OwnerId = "user-1" };
            context.Characters.Add(character);
            await context.SaveChangesAsync();

            // Act - fetch it back, change a field, save again (mutating the tracked
            // entity directly - same pattern the real Edit controller action uses)
            var toEdit = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
            toEdit.Name = "Aria the Wise";
            await context.SaveChangesAsync();

            // Assert - re-query fresh, confirm the new value stuck, not the old one
            var updated = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
            Assert.Equal("Aria the Wise", updated.Name);
        }

        [Fact]
        public async Task Character_Delete_RemovesFromDatabase()
        {
            // Arrange - save a character first
            using var context = CreateContext();
            var character = new Character { Name = "Aria", Level = 5, Race = "Elf", Class = "Wizard", Backstory = "", OwnerId = "user-1" };
            context.Characters.Add(character);
            await context.SaveChangesAsync();

            // Act
            context.Characters.Remove(character);
            await context.SaveChangesAsync();

            // Assert - querying for that id should come back empty now
            var deleted = await context.Characters.FirstOrDefaultAsync(c => c.Id == character.Id);
            Assert.Null(deleted);
        }

    }
}