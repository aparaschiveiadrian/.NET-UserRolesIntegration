using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Data
{
    // STEP 3: useri si roluri
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> // implicit in loc de <ApplicationUser> este IdentityUser, dar noi vrem sa lucram cu userul definit
        //IdentityDbContext and not just DbContext, because we need user system at base, not just entity framework
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)//dependency injection, type: ApplicationDbContext, DI inject for the received type whatever the name for options is
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
