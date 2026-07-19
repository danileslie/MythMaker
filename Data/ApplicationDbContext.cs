using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MythMaker.Models;

namespace MythMaker.Data
{
    //connection to the database
    //dbcontext -> identitybscontext: swaps base for one that has identity built in
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        // lets the class use the LocalDB connection string locally and a different Azure SQL connection string
        // once deployed
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Character> Characters { get; set; }

        // OwnerId is just a plain string until we say otherwise - this tells EF it's a real FK to
        // IdentityUser, and sets cascade delete so deleting a user takes their characters with it.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // gotta call base first or Identity's own table setup gets wiped out
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Character>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}