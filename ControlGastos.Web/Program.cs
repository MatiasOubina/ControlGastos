using System.Text;
using ControlGastos.Web;
using ControlGastos.Data.Context;
using ControlGastos.Services.Interfaces;
using ControlGastos.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ControlGastosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios de negocio
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISubCategoriaService, SubCategoriaService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<IFormasDePagoService, FormasDePagoService>();
builder.Services.AddScoped<ITiposMovimientoService, TiposMovimientoService>();
builder.Services.AddScoped<ISaldosInicialesService, SaldosInicialesService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Autenticación JWT con token almacenado en cookie HTTP-only
var jwtConfig = builder.Configuration.GetSection("Jwt");
var key       = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtConfig["Issuer"],
        ValidAudience            = jwtConfig["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.Zero
    };

    // Leer el token desde la cookie HTTP-only en lugar del header Authorization
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["auth_token"];
            return Task.CompletedTask;
        },
        // Redirigir al login en lugar de devolver 401
        OnChallenge = context =>
        {
            context.HandleResponse();
            var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Crea el usuario admin inicial si no existe ninguno
await SeedUsuario.CrearAdminSiNoExisteAsync(app.Services);

app.Run();
