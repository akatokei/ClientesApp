using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ClientesApp.Models;

namespace ClientesApp.Services
{
    // Guarda y recupera los clientes en un fichero JSON del disco.
    // Escribe en camelCase ("dni", "fechaModificacion") igual que la API,
    // así los dos programas pueden compartir el mismo fichero.
    public class AlmacenClientes
    {
        private static readonly JsonSerializerSettings _ajustes = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(), // nombres en camelCase
            Formatting = Formatting.Indented                                  // JSON legible
        };

        private readonly string _rutaFichero;

        // Constructor normal: (carpeta del .exe)\data\clientes_store.json
        public AlmacenClientes()
            : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "clientes_store.json"))
        {
        }

        // Constructor con ruta propia (lo usan los tests con un fichero temporal)
        public AlmacenClientes(string rutaFichero)
        {
            _rutaFichero = rutaFichero;

            // Si la carpeta no existe, la creamos
            Directory.CreateDirectory(Path.GetDirectoryName(rutaFichero));
        }

        // Lee los clientes del fichero. Si no existe, devuelve una lista vacía.
        // Si el fichero existe pero no tiene un formato JSON válido, lanza FormatException
        // con un mensaje claro (FormPrincipal lo enseña al usuario).
        public List<ClienteDesktop> Cargar()
        {
            if (!File.Exists(_rutaFichero))
                return new List<ClienteDesktop>();

            string texto = File.ReadAllText(_rutaFichero);

            try
            {
                return JsonConvert.DeserializeObject<List<ClienteDesktop>>(texto, _ajustes)
                       ?? new List<ClienteDesktop>();
            }
            catch (JsonException)
            {
                // JsonException cubre tanto "JSON mal escrito" como "JSON que no es una lista de clientes"
                throw new FormatException(
                    "Formato incorrecto: el fichero guardado no tiene la estructura esperada (una lista de clientes en JSON).");
            }
        }

        // Escribe los clientes en el fichero (sustituye lo que hubiera)
        public void Guardar(List<ClienteDesktop> clientes)
        {
            File.WriteAllText(_rutaFichero, JsonConvert.SerializeObject(clientes, _ajustes));
        }
    }
}