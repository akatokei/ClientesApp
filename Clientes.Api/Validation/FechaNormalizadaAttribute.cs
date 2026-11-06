using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClientesApi.Validation;

// Validador propio: la fecha debe estar escrita en formato dd/MM/yyyy.
// Ejemplos válidos: 25/12/1990 y 5/3/1990.
// Rechaza fechas imposibles (30/02/3010), otros formatos (1990-12-25),
// fechas futuras y fechas con más de 130 años.
// Si el campo está vacío NO se queja (la fecha es opcional).
[AttributeUsage(AttributeTargets.Property)]
public class FechaNormalizadaAttribute : ValidationAttribute
{
    public FechaNormalizadaAttribute()
    {
        ErrorMessage = "La fecha debe tener formato dd/MM/yyyy y no superar los 130 años.";
    }

    public override bool IsValid(object? value)
    {
        string? texto = value as string;

        if (string.IsNullOrEmpty(texto)) return true;

        return EsValida(texto);
    }

    public static bool EsValida(string texto)
    {
        // "d/M/yyyy" acepta tanto "5/3/1990" como "05/03/1990".
        if (!DateTime.TryParseExact(
            texto, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            return false;

        // La fecha no puede ser futura ni tener más de 130 años.
        DateTime hoy = DateTime.Today;

        return fecha <= hoy && fecha >= hoy.AddYears(-130);
    }
}