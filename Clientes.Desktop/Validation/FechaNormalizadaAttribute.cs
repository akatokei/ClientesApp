using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ClientesApp.Validation
{
    // Validador propio: la fecha debe estar escrita en formato dd/MM/yyyy.
    // Válidas: 25/12/1990 y 5/3/1990.
    // Rechaza fechas imposibles y otros formatos.
    // No permite fechas futuras ni anteriores a 130 años.
    // Si el campo está vacío, no se queja (la fecha es opcional).
    public class FechaNormalizadaAttribute : ValidationAttribute
    {
        public FechaNormalizadaAttribute()
        {
            ErrorMessage = "La fecha debe tener el formato dd/MM/yyyy y estar dentro del rango permitido (últimos 130 años).";
        }

        public override bool IsValid(object value)
        {
            string texto = value as string;

            // La fecha es opcional.
            if (string.IsNullOrEmpty(texto))
                return true;

            return EsValida(texto);
        }

        public static bool EsValida(string texto)
        {
            // Acepta tanto "5/3/1990" como "05/03/1990".
            DateTime fecha;

            if (!DateTime.TryParseExact(texto, "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return false;
            }

            DateTime hoy = DateTime.Today;

            // No se permiten fechas futuras.
            if (fecha > hoy)
                return false;

            // No se permiten fechas anteriores a 130 años.
            DateTime fechaMinima = hoy.AddYears(-130);

            if (fecha < fechaMinima)
                return false;

            return true;
        }
    }
}