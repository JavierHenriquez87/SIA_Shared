using System;
using Microsoft.EntityFrameworkCore;
using SIA.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseOracle(builder.Configuration.GetConnectionString("SIA_DEV"),
    opt => opt.UseOracleSQLCompatibility("11")));


builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromHours(16);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
