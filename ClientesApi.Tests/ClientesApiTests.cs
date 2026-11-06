using ClientesApi.Models;
using ClientesApi.Services;
using ClientesApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ClientesApi.Tests;

public class ClientesApiTests : IDisposable
{
    private readonly string _fichero;
    private readonly WebApplicationFactory<Program> _fabrica;
    private readonly HttpClient _http;

    public ClientesApiTests()
    {
        // Cada test usa su propio fichero temporal, para no mezclar datos
        _fichero = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        // Arrancamos la API real, cambiando solo el almacén por uno con fichero temporal
        _fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(servicios =>
            {
                servicios.RemoveAll<IClienteStore>();
                servicios.AddSingleton<IClienteStore>(new ClienteStore());
            }));

        _http = _fabrica.CreateClient();
    }

    public void Dispose()
    {
        _http.Dispose();
        _fabrica.Dispose();
        if (File.Exists(_fichero)) File.Delete(_fichero);
    }

    // Un cliente correcto, tal y como lo mandaría un programa por HTTP
    private static object ClienteValido(string dni = "12345678Z") => new
    {
        dni,
        nombre = "Ana",
        apellidos = "Pérez Gil",
        fechaNacimiento = "23/05/1990",
        telefono = "600123456",
        email = "ana@gmail.com"
    };

    // ---------- GET ----------
    [Fact]
    public async Task Get_SinClientes_Devuelve200YListaVacia()
    {
        var respuesta = await _http.GetAsync("/clientes");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var lista = await respuesta.Content.ReadFromJsonAsync<List<ClienteDto>>();
        Assert.Empty(lista!);
    }

    [Fact]
    public async Task Get_DniInexistente_Devuelve404()
    {
        var respuesta = await _http.GetAsync("/clientes/00000000T");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_NoDevuelveIdNiFechaModificacion()
    {
        await _http.PostAsJsonAsync("/clientes", ClienteValido());

        string unCliente = await (await _http.GetAsync("/clientes/12345678Z")).Content.ReadAsStringAsync();
        string lista = await (await _http.GetAsync("/clientes")).Content.ReadAsStringAsync();

        foreach (string json in new[] { unCliente, lista })
        {
            Assert.DoesNotContain("\"id\"", json);
            Assert.DoesNotContain("fechaModificacion", json, StringComparison.OrdinalIgnoreCase);
        }
    }

    // ---------- POST ----------
    [Fact]
    public async Task Post_ClienteValido_Devuelve201YSePuedeConsultar()
    {
        var respuesta = await _http.PostAsJsonAsync("/clientes", ClienteValido());

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.EndsWith("/clientes/12345678Z", respuesta.Headers.Location!.ToString());

        var consulta = await _http.GetAsync("/clientes/12345678Z");
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
    }

    [Fact]
    public async Task Post_DniRepetido_Devuelve400()
    {
        await _http.PostAsJsonAsync("/clientes", ClienteValido());

        var respuesta = await _http.PostAsJsonAsync("/clientes", ClienteValido());

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }


    [Fact]
    public async Task Post_DniConLetraIncorrecta_Devuelve400IndicandoElCampo()
    {
        var respuesta = await _http.PostAsJsonAsync("/clientes", ClienteValido("12345678A"));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        string cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("Dni", cuerpo);
    }


    // ---------- DELETE ----------
    [Fact]
    public async Task Delete_Existente_Devuelve204YYaNoSeEncuentra()
    {
        await _http.PostAsJsonAsync("/clientes", ClienteValido());

        var respuesta = await _http.DeleteAsync("/clientes/12345678Z");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
        var consulta = await _http.GetAsync("/clientes/12345678Z");
        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);
    }

    [Fact]
    public async Task Delete_Inexistente_Devuelve404()
    {
        var respuesta = await _http.DeleteAsync("/clientes/00000000T");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }
}