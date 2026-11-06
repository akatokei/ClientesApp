using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ClientesApp.Validation
{
    // Validador propio: comprueba que un DNI (12345678Z) o un NIE de
    // extranjero (X1234567L) sea CORRECTO, es decir, que su letra final cuadre.
    //
    // Se usa poniendo [DniNie] encima de una propiedad.
    // Si el campo está vacío NO se queja: de eso ya se encarga [Required].
    public class DniNieAttribute : ValidationAttribute
    {
        private const string Letras = "TRWAGMYFPDXBNJZSQVHLCKE";

        public DniNieAttribute()
        {
            ErrorMessage = "12345678Z o X1234567L";
        }

        // Valida el valor de la propiedad
        public override bool IsValid(object value)
        {
            string texto = value as string;

            if (string.IsNullOrEmpty(texto)) return true; // vacío: lo decide [Required]

            return EsValido(texto);
        }

        // Lógica pura, separada para poder probarla directamente en los tests
        public static bool EsValido(string texto)
        {
            texto = texto.ToUpperInvariant(); // "12345678z" también se acepta

            // DNI: 8 números + letra.      NIE: X, Y o Z + 7 números + letra.
            bool esDni = Regex.IsMatch(texto, "^[0-9]{8}[A-Z]$");
            bool esNie = Regex.IsMatch(texto, "^[XYZ][0-9]{7}[A-Z]$");
            if (!esDni && !esNie) return false;

            // Nos quedamos con todo menos la letra final
            string numeros = texto.Substring(0, texto.Length - 1);

            // En el NIE la primera letra se cambia por un número: X=0, Y=1, Z=2
            if (esNie)
                numeros = "XYZ".IndexOf(numeros[0]) + numeros.Substring(1);

            int numero = int.Parse(numeros);
            char letraCorrecta = Letras[numero % 23];

            return texto[texto.Length - 1] == letraCorrecta;
        }
    }
}