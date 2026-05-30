namespace ControlGastos.Web.Helpers;

public static class MovimientoHelper
{
    /// <summary>
    /// Clasifica un tipo de movimiento según su descripción.
    /// Convención: la descripción debe contener "ingreso", "egreso"/"gasto" o "pasaje"/"transfer"/"traspaso".
    /// </summary>
    public static string GetClasificacion(string descripcionTipo)
    {
        var d = descripcionTipo.ToLowerInvariant();
        if (d.Contains("ingreso") || d.Contains("entrada"))             return "Ingreso";
        if (d.Contains("egreso")  || d.Contains("gasto") || d.Contains("salida")) return "Egreso";
        if (d.Contains("pasaje")  || d.Contains("transfer") || d.Contains("traspaso")) return "Pasaje";
        return "Otro";
    }

    public static bool EsPasaje(string descripcionTipo)
        => GetClasificacion(descripcionTipo) == "Pasaje";

    /// <summary>CSS de color para el monto y tipo en el listado.</summary>
    public static string GetColorClass(string descripcionTipo)
        => GetClasificacion(descripcionTipo) switch
        {
            "Ingreso" => "text-success fw-semibold",
            "Egreso"  => "text-danger fw-semibold",
            "Pasaje"  => "text-primary fw-semibold",
            _         => "fw-semibold"
        };

    /// <summary>CSS del badge del tipo en la tabla.</summary>
    public static string GetBadgeClass(string descripcionTipo)
        => GetClasificacion(descripcionTipo) switch
        {
            "Ingreso" => "bg-success-subtle text-success",
            "Egreso"  => "bg-danger-subtle text-danger",
            "Pasaje"  => "bg-primary-subtle text-primary",
            _         => "bg-secondary-subtle text-secondary"
        };

    /// <summary>Monto formateado con signo y símbolo.</summary>
    public static string FormatMonto(decimal monto, string descripcionTipo)
        => GetClasificacion(descripcionTipo) switch
        {
            "Ingreso" => $"+ $ {monto:N2}",
            "Egreso"  => $"- $ {monto:N2}",
            "Pasaje"  => $"↔ $ {monto:N2}",
            _         => $"$ {monto:N2}"
        };

    /// <summary>
    /// Monto con signo para cálculo de balance:
    /// Ingreso = positivo, Egreso = negativo, Pasaje = 0 (neutro).
    /// </summary>
    public static decimal GetMontoSignado(decimal monto, string descripcionTipo)
        => GetClasificacion(descripcionTipo) switch
        {
            "Ingreso" =>  monto,
            "Egreso"  => -monto,
            _         =>  0m
        };
}
