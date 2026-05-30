using ControlGastos.Data.Context;
using ControlGastos.Services.Interfaces;
using ControlGastos.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ControlGastosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISubCategoriaService, SubCategoriaService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<IFormasDePagoService, FormasDePagoService>();
builder.Services.AddScoped<ITiposMovimientoService, TiposMovimientoService>();
builder.Services.AddScoped<ISaldosInicialesService, SaldosInicialesService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
