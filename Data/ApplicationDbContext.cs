using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    }
}