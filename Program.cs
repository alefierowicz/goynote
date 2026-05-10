using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using goynote.Models.System;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
string? connectionString = builder.Configuration.GetConnectionString("GoynoteDB");

var getDbContext = (DbContextOptionsBuilder o) =>
{
    bool isLocalDb = connectionString?.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ?? false;
    bool canUseSqlServer = !string.IsNullOrEmpty(connectionString) && (!isLocalDb || RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

    if (canUseSqlServer)
    {
        o.UseSqlServer(connectionString);
    }
    else
    {
        o.UseSqlite("Data Source=goynote.db");
    }
};

builder.Services.AddDbContext<IdentityDatabaseContext>(getDbContext);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IdentityDatabaseContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
