
# Users and Roles Integration in a .NET Project

## Step 1: Create `ApplicationUser` Class
Create a class `ApplicationUser` that derives from `IdentityUser`.
```csharp
using Microsoft.AspNetCore.Identity;

namespace ArticlesApp.Models
{
    public class ApplicationUser : IdentityUser
    {
    }
}
```

## Step 2: Modify `Program.cs` to Use `ApplicationUser`
Update `Program.cs` to use `ApplicationUser` for identity and include roles.
```csharp
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
    options.SignIn.RequireConfirmedAccount = true) 
    // Sessions, cookies, etc. This helps configure identity.
    .AddRoles<IdentityRole>() 
    // Store roles; we use `IdentityRole` instead of extending it as the default is sufficient.
    .AddEntityFrameworkStores<ApplicationDbContext>(); 
    // Save data in the database context.
```

## Step 3: Update `ApplicationDbContext`
Modify `ApplicationDbContext` to use `ApplicationUser` instead of `IdentityUser`.
```csharp
using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        // `IdentityDbContext` is used instead of `DbContext` to include the user system base.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
```

## Step 4: Add `SeedData` Class
Create a class `SeedData` to seed roles and users.
```csharp
using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArticlesApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                if (context.Roles.Any()) 
                {
                    // If roles already exist, return to prevent duplicates.
                    return;
                }

                // Add roles
                context.Roles.AddRange(
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "Admin".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "Editor", NormalizedName = "Editor".ToUpper() },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7212", Name = "User", NormalizedName = "User".ToUpper() }
                );

                var hasher = new PasswordHasher<ApplicationUser>();

                // Add users
                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                        UserName = "admin@test.com",
                        Email = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                        UserName = "editor@test.com",
                        Email = "editor@test.com",
                        NormalizedUserName = "EDITOR@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "Editor1!")
                    },
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                        UserName = "user@test.com",
                        Email = "user@test.com",
                        NormalizedUserName = "USER@TEST.COM",
                        PasswordHash = hasher.HashPassword(null, "User1!")
                    }
                );

                // Assign roles to users
                context.UserRoles.AddRange(
                    new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0" },
                    new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1" },
                    new IdentityUserRole<string> { RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212", UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2" }
                );

                context.SaveChanges();
            }
        }
    }
}
```

## Step 5: Initialize Seeder in `Program.cs`
Add the following code after building the app to initialize the seeder.
```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}
```

## Step 6: Update Models
Add properties for user relationships in models.
### `Article.cs`
```csharp
public string? UserId { get; set; }
public virtual ApplicationUser? User { get; set; }
```

### `Comment.cs`
```csharp
public string? UserId { get; set; }
public virtual ApplicationUser? User { get; set; }
```

### `ApplicationUser.cs`
```csharp
public virtual ICollection<Comment>? Comments { get; set; }
public virtual ICollection<Article>? Articles { get; set; }
```

## Step 7: Modify `_LoginPartial.cshtml`
Update dependency injection in `_LoginPartial.cshtml`.
```razor
@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager
```

## Step 8: Run Migrations
Generate and apply migrations.
```bash
Add-Migration UserRoles
Update-Database
```

## Step 9: Scaffold and Customize Functionality
Use scaffolding to customize functionality.
1. Add a new scaffolded item from Solution Explorer.
2. Modify the generated `Register.cshtml.cs` file to assign a default role to newly registered users.
```csharp
await _userManager.AddToRoleAsync(user, "User");
```
