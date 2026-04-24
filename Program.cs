using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using goynote.Models.System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
string? connectionString = builder.Configuration.GetConnectionString("GoynoteDB");

builder.Services.AddDbContext<DatabaseContext>(o =>
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
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
