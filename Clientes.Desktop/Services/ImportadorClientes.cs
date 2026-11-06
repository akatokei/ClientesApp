using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ClientesApp.Models;

namespace ClientesApp.Services
{
    // Lee un fichero CSV o JSON y devuelve los clientes que contiene.
    //
    // NO valida ni rechaza nada: si un dato está mal o falta, el cliente se importa
    // igualmente y la rejilla lo marcará en rojo. Así nunca se bloquea la importación.
    // Si el fichero está roto del todo (ej: JSON mal escrito) lanza una excepción,
    // que FormPrincipal captura para avisar al usuario.
    public class ImportadorClientes
    {
        // ---------- CSV ----------
        // "progreso" avisa a la barra de progreso (valores de 0 a 100)
        public List<ClienteDesktop> ImportarCsv(string ruta, IProgress<int> progreso)
        {
            var clientes = new List<ClienteDesktop>();

            // Leemos todas las líneas del fichero
            string[] lineas = File.ReadAllLines(ruta);

            // La línea 0 es la cabecera (dni,nombre,...), así que empezamos en la 1
            int total = lineas.Length - 1;

            for (int i = 1; i < lineas.Length; i++)
            {
                // Las líneas vacías se saltan
                if (!string.IsNullOrWhiteSpace(lineas[i]))
                {
                    // Cortamos por comas: dni,nombre,apellidos,fechaNacimiento,telefono,email
                    string[] partes = lineas[i].Split(',');

                    // Id y FechaModificacion se generan solos al crear el cliente
                    clientes.Add(new ClienteDesktop
                    {
                        Dni = Columna(partes, 0),
                        Nombre = Columna(partes, 1),
                        Apellidos = Columna(partes, 2),
                        FechaNacimiento = Columna(partes, 3),
                        Telefono = Columna(partes, 4),
                        Email = Columna(partes, 5)
                    });
                }

                // Avisamos del progreso (porcentaje de líneas ya leídas)
                progreso.Report(i * 100 / total);
            }

            return clientes;
        }

        // ---------- JSON ----------
        // Si el JSON está mal escrito o no es una lista de clientes, lanza FormatException.
        public List<ClienteDesktop> ImportarJson(string ruta, IProgress<int> progreso)
        {
            string texto = File.ReadAllText(ruta);
            List<ClienteDesktop> clientes;

            try
            {
                // Convertimos el texto JSON en una lista de clientes de golpe
                clientes = JsonConvert.DeserializeObject<List<ClienteDesktop>>(texto)
                           ?? new List<ClienteDesktop>();
            }
            catch (JsonException)
            {
                throw new FormatException(
                    "Formato incorrecto: el fichero JSON no tiene la estructura esperada (una lista de clientes).");
            }

            for (int i = 0; i < clientes.Count; i++)
                progreso.Report((i + 1) * 100 / clientes.Count);

            return clientes;
        }

        // ---------- Ayudante ----------
        // Devuelve la columna "posicion" de una línea, o "" si la línea es más corta
        private static string Columna(string[] partes, int posicion)
        {
            return posicion < partes.Length ? partes[posicion].Trim() : "";
        }
    }
}