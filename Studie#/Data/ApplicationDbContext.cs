using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Studie_.Models;

namespace Studie_.Data
{
    public class ApplicationDbContext : IdentityDbContext<InstitutionUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public DbSet <Institutions> Institutions { get; set; }
    }
}
