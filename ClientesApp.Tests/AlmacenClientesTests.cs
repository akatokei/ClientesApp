using System;
using System.Collections.Generic;
using System.IO;
using ClientesApp.Models;
using ClientesApp.Services;
using Xunit;

namespace ClientesApp.Tests
{
    // Prueba guardar y cargar con un fichero REAL en una carpeta temporal
    // (no toca tus datos de verdad).
    public class AlmacenClientesTests : IDisposable
    {
        private readonly string _carpeta;
        private readonly string _ruta;

        public AlmacenClientesTests()
        {
            _carpeta = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            _ruta = Path.Combine(_carpeta, "data", "clientes_store.json");
        }

        public void Dispose()
        {
            if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, true);
        }

        private static ClienteDesktop UnCliente(string dni)
        {
            return new ClienteDesktop
            {
                Dni = dni,
                Nombre = "Ana",
                Apellidos = "Pérez",
                FechaNacimiento = "23/05/1990",
                Telefono = "600123456",
                Email = "ana@gmail.com"
            };
        }

        [Fact]
        public void Cargar_SinFichero_DevuelveListaVacia()
        {
            Assert.Empty(new AlmacenClientes(_ruta).Cargar());
        }

        [Fact]
        public void GuardarYCargar_DevuelveLosMismosClientes()
        {
            var almacen = new AlmacenClientes(_ruta);
            almacen.Guardar(new List<ClienteDesktop> { UnCliente("12345678Z"), UnCliente("X1234567L") });

            // Un almacén nuevo (como si se reabriera la app) lee lo guardado
            List<ClienteDesktop> cargados = new AlmacenClientes(_ruta).Cargar();

            Assert.Equal(2, cargados.Count);
            Assert.Equal("12345678Z", cargados[0].Dni);
            Assert.Equal("X1234567L", cargados[1].Dni);
            Assert.Equal("Pérez", cargados[0].Apellidos);
        }

        [Fact]
        public void GuardarYCargar_ConservaElIdYLaFechaDeModificacion()
        {
            var cliente = UnCliente("12345678Z");
            cliente.FechaModificacion = new DateTime(2026, 11, 5, 18, 22, 31);
            var almacen = new AlmacenClientes(_ruta);
            almacen.Guardar(new List<ClienteDesktop> { cliente });

            ClienteDesktop cargado = almacen.Cargar()[0];
        }


        [Fact]
        public void Guardar_SobrescribeLoAnterior()
        {
            var almacen = new AlmacenClientes(_ruta);
            almacen.Guardar(new List<ClienteDesktop> { UnCliente("12345678Z"), UnCliente("X1234567L") });

            almacen.Guardar(new List<ClienteDesktop> { UnCliente("00000000T") });

            List<ClienteDesktop> cargados = almacen.Cargar();
            Assert.Single(cargados);
            Assert.Equal("00000000T", cargados[0].Dni);
        }

        [Fact]
        public void LeeUnFicheroEscritoPorLaApi()
        {
            var almacen = new AlmacenClientes(_ruta);
            File.WriteAllText(_ruta, @"[
                { ""id"": ""11111111-1111-1111-1111-111111111111"",
                  ""dni"": ""12345678Z"", ""nombre"": ""Ana"", ""apellidos"": ""Pérez"",
                  ""fechaNacimiento"": ""23/05/1990"", ""telefono"": ""600123456"", ""email"": ""ana@gmail.com"",
                  ""fechaModificacion"": ""2026-11-05T18:22:31"" }
            ]");

            List<ClienteDesktop> cargados = almacen.Cargar();

            Assert.Single(cargados);
            Assert.Equal("12345678Z", cargados[0].Dni);
        }
    }
}