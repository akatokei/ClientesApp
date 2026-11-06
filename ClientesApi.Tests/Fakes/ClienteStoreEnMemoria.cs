using ClientesApi.Models;
using ClientesApi.Services;
using ClientesApi.Services.Interfaces;

namespace ClientesApi.Tests.Fakes;

// Almacén "de mentira" que guarda en memoria, sin ficheros.
// Sirve para probar el controlador.
public class ClienteStoreEnMemoria : IClienteStore
{
    private readonly List<Cliente> _lista = new();

    public List<Cliente> Listar() => _lista.ToList();

    public Cliente? Buscar(string dni) =>
        _lista.FirstOrDefault(c => string.Equals(c.Dni, dni, StringComparison.OrdinalIgnoreCase));

    public bool Agregar(Cliente nuevo)
    {
        if (Buscar(nuevo.Dni) != null) return false;
        _lista.Add(nuevo);
        return true;
    }

    public bool Borrar(string dni) =>
        _lista.RemoveAll(c => string.Equals(c.Dni, dni, StringComparison.OrdinalIgnoreCase)) > 0;
}