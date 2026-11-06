using ClientesApi.Models;
using ClientesApi.Services;
using Xunit;

namespace ClientesApi.Tests;

// Prueba el almacén con un fichero REAL en una carpeta temporal
// que se borra al terminar cada test (no toca tus datos).
public class ClienteStoreTests : IDisposable
{
    private readonly string _carpeta;
    private readonly string _ruta;

    public ClienteStoreTests()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _ruta = Path.Combine(_carpeta, "data", "clientes_store.json");
    }

    // xUnit llama a Dispose al acabar cada test: limpiamos la carpeta temporal
    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, true);
    }

    private ClienteStore NuevoStore() => new();

    private static Cliente UnCliente(string dni = "12345678Z") => new()
    {
        Dni = dni,
        Nombre = "Ana",
        Apellidos = "Pérez",
        FechaNacimiento = "23/05/1990",
        Telefono = "600123456",
        Email = "ana@gmail.com"
    };

    [Fact]
    public void Listar_SinFichero_DevuelveListaVacia()
    {
        Assert.Empty(NuevoStore().Listar());
    }

    [Fact]
    public void Agregar_ClienteNuevo_DevuelveTrueYApareceEnElListado()
    {
        var store = NuevoStore();

        Assert.True(store.Agregar(UnCliente()));
        Assert.Single(store.Listar());
    }

    [Fact]
    public void Agregar_DniRepetido_DevuelveFalseYNoDuplica()
    {
        var store = NuevoStore();
        store.Agregar(UnCliente());

        Assert.False(store.Agregar(UnCliente()));
        Assert.Single(store.Listar());
    }

    [Fact]
    public void Agregar_MismoDniConOtraMayuscula_CuentaComoRepetido()
    {
        var store = NuevoStore();
        store.Agregar(UnCliente("12345678Z"));

        Assert.False(store.Agregar(UnCliente("12345678z")));
    }

    [Fact]
    public void Buscar_DniInexistente_DevuelveNull()
    {
        Assert.Null(NuevoStore().Buscar("00000000T"));
    }

    [Fact]
    public void Buscar_NoDistingueMayusculasEnElDni()
    {
        var store = NuevoStore();
        store.Agregar(UnCliente("12345678Z"));

        Assert.NotNull(store.Buscar("12345678z"));
    }

    [Fact]
    public void Borrar_DniExistente_DevuelveTrueYYaNoEsta()
    {
        var store = NuevoStore();
        store.Agregar(UnCliente());

        Assert.True(store.Borrar("12345678Z"));
        Assert.Empty(store.Listar());
    }

    [Fact]
    public void Borrar_DniInexistente_DevuelveFalse()
    {
        Assert.False(NuevoStore().Borrar("00000000T"));
    }

    [Fact]
    public void Borrar_SoloQuitaElClienteIndicado()
    {
        var store = NuevoStore();
        store.Agregar(UnCliente("12345678Z"));
        store.Agregar(UnCliente("00000000T"));

        store.Borrar("12345678Z");

        var quedan = store.Listar();
        Assert.Single(quedan);
        Assert.Equal("00000000T", quedan[0].Dni);
    }

    [Fact]
    public void LosDatosSobrevivenAUnReinicio_ConSuIdYSuFecha()
    {
        var original = UnCliente();
        original.FechaModificacion = new DateTime(2026, 11, 5, 18, 22, 31);
        NuevoStore().Agregar(original);

        // Otro store nuevo (como si se reiniciara la API) lee del disco
        var leido = NuevoStore().Listar().Single();

        Assert.Equal(original.Id, leido.Id);
        Assert.Equal(original.FechaModificacion, leido.FechaModificacion);
    }

    [Fact]
    public void LeeUnFicheroEscritoPorLaAppDeEscritorio()
    {
        var store = NuevoStore();
        File.WriteAllText(_ruta, """
            [
              { "id": "11111111-1111-1111-1111-111111111111",
                "dni": "12345678Z", "nombre": "Ana", "apellidos": "Pérez",
                "fechaNacimiento": "23/05/1990", "telefono": "600123456", "email": "ana@gmail.com",
                "fechaModificacion": "2026-11-05T18:22:31" }
            ]
            """);

        var clientes = store.Listar();

        Assert.Single(clientes);
        Assert.Equal("12345678Z", clientes[0].Dni);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), clientes[0].Id);
    }
}