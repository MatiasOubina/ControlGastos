using ControlGastos.Services.Interfaces;

namespace ControlGastos.Web;
#pragma warning disable CS8019

public static class SeedUsuario
{
    public static async Task CrearAdminSiNoExisteAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    }
}
