using ASCWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASCWeb.Configuration;
using ASCWeb.Services;
using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>((options) =>
{
    options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddScoped<DbContext, ApplicationDbContext>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddOptions();
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddControllersWithViews();

//Add application services
builder.Services.AddTransient<IEmailSender, AuthMessageSender>();
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();
builder.Services.AddSingleton<IIdentitySeed, IdentitySeed>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddRazorPages();

var app = builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();
    await storageSeed.Seed(
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<ApplicationSettings>>());
}

app.Run();