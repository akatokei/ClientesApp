using ClientesApi.Models;

namespace ClientesApi.Services.Interfaces;

// Un "contrato": lista lo que el almacén sabe hacer, sin decir cómo.
// El controlador solo conoce este contrato, no si por detrás hay un fichero o una base de datos.
public interface IClienteStore
{
    List<Cliente> Listar();              // todos los clientes
    Cliente? Buscar(string dni);         // uno por DNI (null si no existe)
    bool Agregar(Cliente nuevo);         // false si el DNI ya existía
    bool Borrar(string dni);             // false si no existía
}