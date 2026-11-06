using System;
using System.ComponentModel.DataAnnotations;
using ClientesApp.Validation;

namespace ClientesApp.Models
{
    // Un cliente. Cada propiedad con [Required] es obligatoria (se marca con * en la rejilla).
    //
    // IMPORTANTE: en ErrorMessage NO va un mensaje de error sino el EJEMPLO CORRECTO.
    // Es lo que la ventana enseña en rojo cuando un dato no cumple la regla.
    //
    // Obligatorios: Dni, Nombre, Apellidos.
    // Opcionales: FechaNacimiento, Telefono, Email (vacíos no se validan,
    // pero si se rellenan deben tener formato correcto).
    public class ClienteDesktop
    {
        // ---- Datos INTERNOS: se guardan en el JSON pero NO hay columna en la rejilla ----

        // Identificador único aleatorio (lo genera .NET con Guid.NewGuid)
        public Guid Id { get; set; } = Guid.NewGuid();

        // Fecha y hora (con segundos) de la última modificación
        public DateTime FechaModificacion { get; set; } = DateTime.Now;

        // ---- Datos que se ven en la rejilla ----

        [Required(ErrorMessage = "12345678Z o X1234567L")]
        [DniNie(ErrorMessage = "12345678Z o X1234567L")]
        public string Dni { get; set; } = "";

        [Required(ErrorMessage = "Ana")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "Pérez García")]
        public string Apellidos { get; set; } = "";

        // Formato normalizado dd/MM/yyyy
        [FechaNormalizada(ErrorMessage = "25/12/1990")]
        public string FechaNacimiento { get; set; } = "";

        // + opcional, números y espacios, de 9 a 15 caracteres
        [RegularExpression(@"^\+?[0-9 ]{9,15}$", ErrorMessage = "600123456")]
        public string Telefono { get; set; } = "";

        // Forma básica: algo@algo.algo (sin espacios)
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "nombre@gmail.com")]
        public string Email { get; set; } = "";
    }
}