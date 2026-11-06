using ClientesApi.Models;
using ClientesApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClientesApi.Controllers;

/// <summary>
/// Gestiona las operaciones de consulta, alta y eliminación de clientes.
/// </summary>
[ApiController]
[Route("clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly IClienteStore _store;

    /// <summary>
    /// Inicializa el controlador con el almacén de clientes.
    /// </summary>
    /// <param name="store">Servicio encargado de gestionar los clientes.</param>
    public ClientesController(IClienteStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Obtiene el listado completo de clientes.
    /// </summary>
    /// <returns>Listado de clientes registrados.</returns>
    /// <response code="200">Listado obtenido correctamente.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteDto>), StatusCodes.Status200OK)]
    public ActionResult<List<ClienteDto>> Listar()
    {
        List<ClienteDto> lista = _store.Listar()
            .Select(ClienteDto.DesdeEntidad)
            .ToList();

        return Ok(lista);
    }

    /// <summary>
    /// Obtiene los datos de un cliente a partir de su DNI.
    /// </summary>
    /// <param name="dni">DNI del cliente que se desea consultar.</param>
    /// <returns>Datos del cliente solicitado.</returns>
    /// <response code="200">Cliente encontrado.</response>
    /// <response code="404">No existe ningún cliente con ese DNI.</response>
    [HttpGet("{dni}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ClienteDto> Obtener(string dni)
    {
        Cliente? cliente = _store.Buscar(dni);

        if (cliente is null)
            return NotFound();

        return Ok(ClienteDto.DesdeEntidad(cliente));
    }

    /// <summary>
    /// Registra un nuevo cliente.
    /// </summary>
    /// <param name="nuevo">Datos del cliente que se desea registrar.</param>
    /// <returns>Datos del cliente creado.</returns>
    /// <response code="201">Cliente creado correctamente.</response>
    /// <response code="400">Los datos no son válidos o el DNI ya está registrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<ClienteDto> Crear(ClienteDto nuevo)
    {
        // Convertimos el DTO en entidad para almacenarlo.
        Cliente entidad = nuevo.AEntidad();

        if (!_store.Agregar(entidad))
            return BadRequest("Ya existe un cliente con ese DNI.");

        // Devolvemos 201 Created con la ubicación del nuevo recurso.
        return CreatedAtAction(
            nameof(Obtener),
            new { dni = entidad.Dni },
            ClienteDto.DesdeEntidad(entidad));
    }

    /// <summary>
    /// Elimina un cliente a partir de su DNI.
    /// </summary>
    /// <param name="dni">DNI del cliente que se desea eliminar.</param>
    /// <response code="204">Cliente eliminado correctamente.</response>
    /// <response code="404">No existe ningún cliente con ese DNI.</response>
    [HttpDelete("{dni}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Borrar(string dni)
    {
        if (!_store.Borrar(dni))
            return NotFound();

        return NoContent();
    }
}