using ArticlesApp.Data;
using ArticlesApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options => //conexiunea cu baza de date
    options.UseSqlServer(connectionString)); // serverul pe care l folosim

builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // folosim pentru a  arunca exceptia in cazul in care nu se gaseste connectionString

//PASUL 2: useri si roluri

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
options.SignIn.RequireConfirmedAccount = true) //sesiuni, cookies, etc( ne ajuta pt configul identitatii)
    .AddRoles<IdentityRole>() //stocam si rolurile, IdentityRole deoarece nu mai are rost sa extindem clasa de role, si o folosim pe cea default
    .AddEntityFrameworkStores<ApplicationDbContext>(); //stocheaza datele in contextul bazei de date

builder.Services.AddControllersWithViews();

var app = builder.Build(); //the instance of our app

//STEP 5: useri si roluri
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Articles}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
