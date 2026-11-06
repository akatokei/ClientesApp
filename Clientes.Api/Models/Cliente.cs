namespace ClientesApi.Models;

// La ENTIDAD: es cada cliente tal y como se guarda en el fichero JSON.
// Lleva dos datos internos (Id y FechaModificacion) que la API NUNCA devuelve
// al exterior: para eso existe ClienteDto.
// Aquí no hay reglas de validación: las reglas están en ClienteDto.
public class Cliente
{
    // Identificador interno único. Lo genera .NET solo (Guid = código aleatorio único).
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Dni { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellidos { get; set; } = "";

    // Texto con formato dd/MM/yyyy (ej: "25/12/1990")
    public string FechaNacimiento { get; set; } = "";

    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";

    // Fecha y hora (con segundos) de la última modificación. Interno.
    public DateTime FechaModificacion { get; set; } = DateTime.Now;
}