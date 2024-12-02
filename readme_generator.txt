USERS AND ROLES
STEP 1: create ApplicationUser as class, deriving from IdentityUser
(In ApplicationUser.cs)

using Microsoft.AspNetCore.Identity;

//PASUL 1: useri si roluri

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        
    }
}


STEP 2: modify Program.cs so we also store roles along the user with the identity of ApplicationUser instead of IdentityUser
(In Program.cs before Build) 
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
options.SignIn.RequireConfirmedAccount = true) //sesiuni, cookies, etc( ne ajuta pt configul identitatii)
    .AddRoles<IdentityRole>() //stocam si rolurile, IdentityRole deoarece nu mai are rost sa extindem clasa de role, si o folosim pe cea default
    .AddEntityFrameworkStores<ApplicationDbContext>(); //stocheaza datele in contextul bazei de date

STEP 3: once we activated these mecanisms and added the services in program.cs, they  store in dbcontext and we have to modify applicationdbcontext so it receives the user type
!the context is the central point from where we can interact with the database in entity framework core
(In ApplicationDbContext.cs)
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

STEP 4:  add a new class in models SeedData.cs

using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider) // the object that we wait for DI
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService
                <DbContextOptions<ApplicationDbContext>>()))
            //using frees memory
            {
                // Check if the database already contains at least one role.
                // If it does, the method returns to prevent re-inserting roles.
                // This ensures that this code is executed only once.
                if (context.Roles.Any()) // Roles is DbSet<IdentityRole>
                {
                    return; // The database already contains roles.
                }

                // Creating roles in the database if they do not exist.
                context.Roles.AddRange(
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() }
                );

                // Creating a new instance to hash user passwords.
                var hasher = new PasswordHasher<ApplicationUser>();

                // Creating users in the database, one for each role.
                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // Primary key
                        UserName = "admin@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1", // Primary key
                        UserName = "editor@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "EDITOR@TEST.COM",
                        Email = "editor@test.com",
                        NormalizedUserName = "EDITOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Editor1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2", // Primary key
                        UserName = "user@test.com",
                        EmailConfirmed = true,
                        NormalizedEmail = "USER@TEST.COM",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User1!")
                    }
                );

                // Assigning users to their respective roles.
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                    },
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212",
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                    }
                );

                // Save changes to the database.
                context.SaveChanges();
            }
        }
    }
}

STEP 5: in Program.cs
we create a scope with seperate memory scope
we do this so when we a have a new reuqest, we dont repeat this process to consume resources again, when the app is started, the instance is created, we create a variable scope with using(it frees memory after use), scope.ServiceProvide being used for DI
when the seeder is running, it verifies if it exists or not, if it doesnt exist, it creates them, if it exists it stops and frees the memory and closes the database conexion

right after building the app
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}


STEP 6: we add the properties in the classes defined in Model:
In Articles.cs:

public string? UserId { get; set; }

but we also have to add the virtual properties
Why? because i need to make joins, access tables, properties etc

public virtual ApplicationUser? User { get; set; }

In Comment.cs:

//STEP 6: useri si roluri
//cheie externa (FK) - un comentariu e postat de catre un user
public string? UserId { get; set; }
//proprietatea virtuala- un comentariu e postat de catre un user
public virtual ApplicationUser? User { get; set; }


In ApplicationUser.cs

using ArticlesApp.Controllers;
using Microsoft.AspNetCore.Identity;

//PASUL 1: useri si roluri

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        //STEP 6: useri si roluri
        //un user poate posta mai multe comentarii
        public virtual ICollection<Comment>? Comments { get; set; }

        //un user poate posta mai multe articole
        public virtual ICollection<Article>? Articles { get; set; }
    }
}


STEP 7:

in Views -> Shared -> _LoginPartial.cshtml we modify

@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager

into

@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager

STEP 8:

Add-Migration UserRoles
Update-Database


STEP 9: in order to modify functionalities which are implemented in the framework, we can use Add Scaffolded Item option, with its help we can bring the source code in the folders

Solution Explorer -> Add -> New Scaffolded Item

After the Account page is created

Areas -> Pages -> Account -> Register.cshtml -> Register.cshtml.cs

inside if(result.Succeeded), after _logger.LogInformation("User created a new account with password.");
type
await _userManager.AddToRoleAsync(user, "User");
