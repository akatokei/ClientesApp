using ClientesApi.Models;
using ClientesApi.Services.Interfaces;
using System.Text.Json;

namespace ClientesApi.Services;

// Cumple el contrato IClienteStore guardando los clientes en un fichero JSON.
public class ClienteStore : IClienteStore
{
    // Opciones de JSON: nombres en camelCase ("dni"), acepta mayúsculas/minúsculas al leer
    // y escribe el fichero con sangrías para que sea legible.
    private static readonly JsonSerializerOptions _opciones =
        new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly string _ruta;


    public ClienteStore()
    {
        // Fichero: (carpeta de la app)/data/clientes_store.json
        string carpeta = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(carpeta); // si la carpeta no existe, se crea
        _ruta = Path.Combine(carpeta, "clientes_store.json");
    }

    public List<Cliente> Listar()
    {
        return Leer();
    }

    public Cliente? Buscar(string dni)
    {
         return Leer().FirstOrDefault(c => c.Dni == dni); 
    }

    public bool Agregar(Cliente nuevo)
    {
        var lista = Leer();
        if (lista.Any(c => c.Dni == nuevo.Dni)) return false; // DNI repetido

        lista.Add(nuevo);
        Escribir(lista);
        return true;
    }

    public bool Borrar(string dni)
    {
            var lista = Leer();
            int borrados = lista.RemoveAll(c => c.Dni == dni);
            if (borrados == 0) return false; // no existía

            Escribir(lista);
            return true;
    }

    // ----- Métodos privados: leer y escribir el fichero -----
    private List<Cliente> Leer()
    {
        if (!File.Exists(_ruta)) return new List<Cliente>();

        string texto = File.ReadAllText(_ruta);
        return JsonSerializer.Deserialize<List<Cliente>>(texto, _opciones) ?? new List<Cliente>();
    }

    private void Escribir(List<Cliente> lista)
    {
        File.WriteAllText(_ruta, JsonSerializer.Serialize(lista, _opciones));
    }
}