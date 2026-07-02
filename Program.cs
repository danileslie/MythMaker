using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MythMaker.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// register Identity's core services (UserManager, SignInManager, password hashing) with no
// bundled UI using AddIdentity, not AddDefaultIdentity, since auth is being built as manual
// MVC controllers/views rather than the scaffolded Razor Pages UI.
// IdentityRole is include because it's part of Identity's baseline schema, even though MythMaker doesn't use roles yet.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// UseAuthentication must run before UseAuthorization -> it establishes who the user is;
// UseAuthorization decides what they're allowed to do. Wrong order means authorization
// checks run against a request that hasn't been identified yet.
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
