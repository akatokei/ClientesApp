using System.ComponentModel.DataAnnotations;
using ClientesApi.Models;
using Xunit;

namespace ClientesApi.Tests;

// Comprueba las reglas del DTO y la conversión DTO <-> entidad.
public class ClienteDtoTests
{
    private static ClienteDto DtoValido() => new()
    {
        Dni = "12345678Z",
        Nombre = "Ana",
        Apellidos = "Pérez Gil",
        FechaNacimiento = "23/05/1990",
        Telefono = "600123456",
        Email = "ana@gmail.com"
    };

    // Devuelve los nombres de los campos que NO cumplen las reglas
    private static List<string> CamposConError(ClienteDto dto)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), resultados, validateAllProperties: true);
        return resultados.SelectMany(r => r.MemberNames).ToList();
    }

    [Fact]
    public void DtoCorrecto_NoTieneErrores()
    {
        var dto = DtoValido();

        Assert.Empty(CamposConError(dto));
    }

    [Fact]
    public void DniConLetraIncorrecta_EsInvalido()
    {
        var dto = DtoValido();
        dto.Dni = "12345678A";

        Assert.Contains(nameof(dto.Dni), CamposConError(dto));
    }

    [Fact]
    public void NieCorrecto_EsValido()
    {
        var dto = DtoValido();
        dto.Dni = "X1234567L";

        Assert.DoesNotContain(nameof(dto.Dni), CamposConError(dto));
    }

    [Theory]
    [InlineData("1990-05-23")]
    [InlineData("30/02/1990")]
    public void FechaMal_EsInvalida(string fecha)
    {
        var dto = DtoValido();
        dto.FechaNacimiento = fecha;

        Assert.Contains(nameof(dto.FechaNacimiento), CamposConError(dto));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345678")]
    [InlineData("abc123456789")]
    public void TelefonoMal_EsInvalido(string telefono)
    {
        var dto = DtoValido();
        dto.Telefono = telefono;

        Assert.Contains(nameof(dto.Telefono), CamposConError(dto));
    }

    [Theory]
    [InlineData("sinarroba")]
    [InlineData("ana@correo")]
    [InlineData("@correo.com")]
    [InlineData("ana @correo.com")]
    public void EmailMal_EsInvalido(string email)
    {
        var dto = DtoValido();
        dto.Email = email;

        Assert.Contains(nameof(dto.Email), CamposConError(dto));
    }

    // Comprueba que se aceptan fechas con día y mes de una o dos cifras
    [Theory]
    [InlineData("5/2/1990")]
    [InlineData("05/02/1990")]
    public void FechaCorrecta_EsValida(string fecha)
    {
        var dto = DtoValido();
        dto.FechaNacimiento = fecha;

        Assert.DoesNotContain(nameof(dto.FechaNacimiento), CamposConError(dto));
    }

    // No se permite una fecha de nacimiento futura
    [Fact]
    public void FechaFutura_EsInvalida()
    {
        var dto = DtoValido();
        dto.FechaNacimiento = DateTime.Today.AddDays(1).ToString("dd/MM/yyyy");

        Assert.Contains(nameof(dto.FechaNacimiento), CamposConError(dto));
    }

    // No se permite una fecha de nacimiento anterior a 130 años
    [Fact]
    public void FechaDeMasDe130Anios_EsInvalida()
    {
        var dto = DtoValido();
        dto.FechaNacimiento = DateTime.Today.AddYears(-131).ToString("dd/MM/yyyy");

        Assert.Contains(nameof(dto.FechaNacimiento), CamposConError(dto));
    }

    // Se permite una fecha de nacimiento de exactamente 130 años
    [Fact]
    public void FechaDentroDelLimiteDe130Anios_EsValida()
    {
        var dto = DtoValido();
        dto.FechaNacimiento = DateTime.Today.AddYears(-130).ToString("dd/MM/yyyy");

        Assert.DoesNotContain(nameof(dto.FechaNacimiento), CamposConError(dto));
    }

    [Fact]
    public void AEntidad_CopiaLosDatosYGeneraIdYFecha()
    {
        var dto = DtoValido();
        var antes = DateTime.Now;

        Cliente entidad = dto.AEntidad();

        Assert.Equal(dto.Dni, entidad.Dni);
        Assert.Equal(dto.Nombre, entidad.Nombre);
        Assert.Equal(dto.FechaNacimiento, entidad.FechaNacimiento);
        Assert.NotEqual(Guid.Empty, entidad.Id);
        Assert.True(entidad.FechaModificacion >= antes.AddSeconds(-1));
    }

    [Fact]
    public void DesdeEntidad_CopiaLosDatos()
    {
        var entidad = new Cliente
        {
            Dni = "12345678Z",
            Nombre = "Ana",
            Apellidos = "Pérez Gil",
            FechaNacimiento = "23/05/1990",
            Telefono = "600123456",
            Email = "ana@gmail.com"
        };

        ClienteDto dto = ClienteDto.DesdeEntidad(entidad);

        Assert.Equal(entidad.Dni, dto.Dni);
        Assert.Equal(entidad.Nombre, dto.Nombre);
        Assert.Equal(entidad.Apellidos, dto.Apellidos);
        Assert.Equal(entidad.FechaNacimiento, dto.FechaNacimiento);
        Assert.Equal(entidad.Telefono, dto.Telefono);
        Assert.Equal(entidad.Email, dto.Email);
    }
}
