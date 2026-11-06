using System.ComponentModel.DataAnnotations;
using ClientesApi.Validation;

namespace ClientesApi.Models;

// DTO = "Data Transfer Object": el cliente tal y como viaja por HTTP.
// Es igual que Cliente pero SIN Id y SIN FechaModificacion (datos internos).
//
// También lleva las reglas de validación. Si una regla falla,
// [ApiController] responde 400 automáticamente, con el mensaje del campo.
//
// Obligatorios: Dni, Nombre, Apellidos.
// Opcionales: FechaNacimiento, Telefono, Email (si vienen vacíos no se validan,
// pero si vienen rellenos deben tener formato correcto).
public class ClienteDto
{
    [Required(ErrorMessage = "El DNI no puede estar vacío.")]
    [DniNie(ErrorMessage = "El DNI/NIE no es válido (ej: 12345678Z o X1234567L).")]
    public string Dni { get; set; } = "";

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    public string Apellidos { get; set; } = "";

    [FechaNormalizada(ErrorMessage = "La fecha debe tener formato dd/MM/yyyy (ej: 25/12/1990).")]
    public string? FechaNacimiento { get; set; } = "";

    // [RegularExpression] ignora los valores vacíos, por eso el campo es opcional.
    // Acepta + opcional, números y espacios, de 9 a 15 caracteres.
    [RegularExpression(@"^\+?[0-9 ]{9,15}$", ErrorMessage = "El teléfono no es válido (ej: 600123456).")]
    public string? Telefono { get; set; } = "";

    // Forma básica: algo@algo.algo  (sin espacios)
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "El email no es válido (ej: nombre@gmail.com).")]
    public string? Email { get; set; } = "";

    // DTO -> entidad. El Id y la FechaModificacion los genera la propia entidad.
    public Cliente AEntidad()
    {
        return new Cliente
        {
            Dni = Dni,
            Nombre = Nombre,
            Apellidos = Apellidos,
            FechaNacimiento = FechaNacimiento ?? "",
            Telefono = Telefono ?? "",
            Email = Email ?? ""
        };
    }

    // Entidad -> DTO. Aquí es donde se "esconden" el Id y la FechaModificacion.
    public static ClienteDto DesdeEntidad(Cliente c)
    {
        return new ClienteDto
        {
            Dni = c.Dni,
            Nombre = c.Nombre,
            Apellidos = c.Apellidos,
            FechaNacimiento = c.FechaNacimiento,
            Telefono = c.Telefono,
            Email = c.Email
        };
    }
}