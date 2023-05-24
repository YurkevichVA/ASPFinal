using ASPFinal.Data;
using ASPFinal.Middleware;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.Data.SqlClient;
using ASPFinal.Services.Hash;
using ASPFinal.Services.Random;
using ASPFinal.Services.Validation;
using ASPFinal.Services.Kdf;
using ASPFinal.Services.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHashService, Md5HashService>();
builder.Services.AddSingleton<IKdfService, HashKdfService>();
builder.Services.AddSingleton<IRandomService, RandomServiceV1>();
builder.Services.AddSingleton<IValidationService, ValidationServiceV1>();
builder.Services.AddSingleton<IEmailService, GmailService>();

string? connectionString = builder.Configuration.GetConnectionString("MySqlDb");
MySqlConnection connection = new(connectionString);
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connection, ServerVersion.AutoDetect(connection)));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();
app.UseSessionAuth();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
