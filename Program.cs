using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZuvoPet_V2.Data;
using ZuvoPet_V2.Helpers;
using ZuvoPet_V2.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.AddSingleton<HelperPathProvider>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IRepositoryZuvoPet_V2, RepositoryZuvoPet_V2>();

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.EnableEndpointRouting = false;
}).AddSessionStateTempDataProvider();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

string connectionString = builder.Configuration.GetConnectionString("ZuvoPet_V2");

builder.Services.AddDbContext<ZuvoPet_V2Context>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(options =>
{
    options.DefaultSignInScheme =
    CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme =
    CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme =
    CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(
    CookieAuthenticationDefaults.AuthenticationScheme,
    config =>
    {
        config.LoginPath = "/Managed/Login";
        config.AccessDeniedPath = "/Managed/Denied";
    });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("SOLOADOPTANTES",
//        policy => policy.RequireRole("Adoptante"));
//    options.AddPolicy("SOLOREFUGIOS",
//        policy => policy.RequireRole("Refugio"));
//    options.AddPolicy("SOLOADMIN",
//        policy => policy.RequireClaim("Admin"));
//});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Adoptante", policy => policy.RequireRole("Adoptante"));
    options.AddPolicy("Refugio", policy => policy.RequireRole("Refugio"));
    options.AddPolicy("Admin", policy => policy.RequireClaim("Admin", "true"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Managed/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Managed}/{action=Landing}/{id?}")
    .WithStaticAssets();
//app.UseMvc(routes =>
//{
//    routes.MapRoute(name: "default",
//    template: "{controller=Managed}/{action=Landing}/{id?}");
//});

app.Run();
