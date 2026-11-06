using System;
using System.Collections.Generic;
using System.IO;
using ClientesApp.Models;
using ClientesApp.Services;
using Xunit;

namespace ClientesApp.Tests
{
    // Un "progreso de mentira" que apunta los números que recibe.
    // (Progress<T> de verdad avisa en otro hilo y haría los tests poco fiables.)
    public class ProgresoFalso : IProgress<int>
    {
        public List<int> Valores = new List<int>();

        public void Report(int valor)
        {
            Valores.Add(valor);
        }
    }

    // Prueba la lectura de CSV y JSON con ficheros REALES en una carpeta temporal.
    public class ImportadorClientesTests : IDisposable
    {
        private const string Cabecera = "dni,nombre,apellidos,fechaNacimiento,telefono,email";

        private readonly string _carpeta;
        private readonly ImportadorClientes _importador = new ImportadorClientes();

        public ImportadorClientesTests()
        {
            _carpeta = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_carpeta);
        }

        // xUnit llama a Dispose al acabar cada test: borramos la carpeta temporal
        public void Dispose()
        {
            Directory.Delete(_carpeta, true);
        }

        private string CrearFichero(string nombre, string contenido)
        {
            string ruta = Path.Combine(_carpeta, nombre);
            File.WriteAllText(ruta, contenido);
            return ruta;
        }

        private static string Lineas(params string[] lineas)
        {
            return string.Join(Environment.NewLine, lineas);
        }

        // =============== CSV ===============

        [Fact]
        public void Csv_DosClientes_LosDevuelveConTodosSusDatos()
        {
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678Z,Ana,Perez,23/05/1990,600123456,ana@gmail.com",
                "X1234567L,Luis,Mora,02/11/1985,611222333,luis@gmail.com"));

            List<ClienteDesktop> clientes = _importador.ImportarCsv(ruta, new ProgresoFalso());

            Assert.Equal(2, clientes.Count);
            Assert.Equal("12345678Z", clientes[0].Dni);
            Assert.Equal("Ana", clientes[0].Nombre);
            Assert.Equal("Perez", clientes[0].Apellidos);
            Assert.Equal("23/05/1990", clientes[0].FechaNacimiento);
            Assert.Equal("600123456", clientes[0].Telefono);
            Assert.Equal("ana@gmail.com", clientes[0].Email);
            Assert.Equal("Luis", clientes[1].Nombre);
        }

        [Fact]
        public void Csv_ClienteConDatosMalos_SeImportaIgualmente()
        {
            // Nada se rechaza: la rejilla lo marcará en rojo
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678A,Pedro,Gomez,1992-03-05,123,pedro@correo"));

            List<ClienteDesktop> clientes = _importador.ImportarCsv(ruta, new ProgresoFalso());

            Assert.Single(clientes);
            Assert.Equal("1992-03-05", clientes[0].FechaNacimiento);
        }

        [Fact]
        public void Csv_LineaConColumnasDeMenos_SeImportaConLosHuecosVacios()
        {
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678Z,Ana,Perez"));

            List<ClienteDesktop> clientes = _importador.ImportarCsv(ruta, new ProgresoFalso());

            Assert.Single(clientes);
            Assert.Equal("Perez", clientes[0].Apellidos);
            Assert.Equal("", clientes[0].FechaNacimiento);
            Assert.Equal("", clientes[0].Email);
        }

        [Fact]
        public void Csv_LineasVacias_SeIgnoran()
        {
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678Z,Ana,Perez,23/05/1990,600123456,ana@gmail.com",
                "",
                "   "));

            Assert.Single(_importador.ImportarCsv(ruta, new ProgresoFalso()));
        }

        [Fact]
        public void Csv_CadaClienteRecibeSuPropioId()
        {
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678Z,Ana,Perez,23/05/1990,600123456,ana@gmail.com",
                "X1234567L,Luis,Mora,02/11/1985,611222333,luis@gmail.com"));

            List<ClienteDesktop> clientes = _importador.ImportarCsv(ruta, new ProgresoFalso());

            Assert.NotEqual(Guid.Empty, clientes[0].Id);
            Assert.NotEqual(clientes[0].Id, clientes[1].Id);
        }

        [Fact]
        public void Csv_LaBarraDeProgresoLlegaAl100()
        {
            string ruta = CrearFichero("c.csv", Lineas(
                Cabecera,
                "12345678Z,Ana,Perez,23/05/1990,600123456,ana@gmail.com",
                "X1234567L,Luis,Mora,02/11/1985,611222333,luis@gmail.com"));
            var progreso = new ProgresoFalso();

            _importador.ImportarCsv(ruta, progreso);

        }

        // =============== JSON ===============

        [Fact]
        public void Json_ClienteCorrecto_LoDevuelve()
        {
            string ruta = CrearFichero("c.json", @"[
                { ""dni"": ""12345678Z"", ""nombre"": ""Ana"", ""apellidos"": ""Perez"",
                  ""fechaNacimiento"": ""23/05/1990"", ""telefono"": ""600123456"", ""email"": ""ana@gmail.com"" }
            ]");

            List<ClienteDesktop> clientes = _importador.ImportarJson(ruta, new ProgresoFalso());

            Assert.Single(clientes);
            Assert.Equal("12345678Z", clientes[0].Dni);
        }

        [Fact]
        public void Json_RegistroSinFecha_SeImportaIgualmente()
        {
            // Justo el caso que antes bloqueaba la importación
            string ruta = CrearFichero("c.json", @"[
                { ""dni"": ""12345678Z"", ""nombre"": ""Ana"", ""apellidos"": ""Perez"" },
                { ""dni"": ""X1234567L"", ""nombre"": ""Luis"", ""apellidos"": ""Mora"", ""fechaNacimiento"": ""mal"" }
            ]");

            List<ClienteDesktop> clientes = _importador.ImportarJson(ruta, new ProgresoFalso());

            Assert.Equal(2, clientes.Count);
        }

        [Fact]
        public void Json_SinId_LeAsignaUnoNuevo()
        {
            string ruta = CrearFichero("c.json", @"[ { ""dni"": ""12345678Z"" } ]");

            List<ClienteDesktop> clientes = _importador.ImportarJson(ruta, new ProgresoFalso());

            Assert.NotEqual(Guid.Empty, clientes[0].Id);
        }

        [Fact]
        public void Json_ConId_ConservaElSuyo()
        {
            string ruta = CrearFichero("c.json",
                @"[ { ""id"": ""11111111-1111-1111-1111-111111111111"", ""dni"": ""12345678Z"" } ]");

            List<ClienteDesktop> clientes = _importador.ImportarJson(ruta, new ProgresoFalso());

            Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), clientes[0].Id);
        }


        [Fact]
        public void Json_FicheroRoto_LanzaExcepcion()
        {
            // El formulario captura esta excepción y le enseña un aviso al usuario
            string ruta = CrearFichero("c.json", "esto no es json {{{");

            Assert.ThrowsAny<Exception>(() => _importador.ImportarJson(ruta, new ProgresoFalso()));
        }

        [Fact]
        public void Json_LaBarraDeProgresoLlegaAl100()
        {
            string ruta = CrearFichero("c.json", @"[ { ""dni"": ""12345678Z"" }, { ""dni"": ""X1234567L"" } ]");
            var progreso = new ProgresoFalso();

            _importador.ImportarJson(ruta, progreso);

        }
    }
}