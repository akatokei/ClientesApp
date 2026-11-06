using System;
using ClientesApp.Models;
using Xunit;

namespace ClientesApp.Tests
{
    public class ClienteDesktopTests
    {
        [Fact]
        public void UnClienteNuevo_TieneUnIdPropio()
        {
            Assert.NotEqual(Guid.Empty, new ClienteDesktop().Id);
        }

        [Fact]
        public void DosClientesNuevos_TienenIdsDistintos()
        {
            Assert.NotEqual(new ClienteDesktop().Id, new ClienteDesktop().Id);
        }

        [Fact]
        public void UnClienteNuevo_TieneComoFechaDeModificacionElMomentoActual()
        {
            DateTime antes = DateTime.Now.AddSeconds(-1);

            var cliente = new ClienteDesktop();

            Assert.True(cliente.FechaModificacion > antes);
            Assert.True(cliente.FechaModificacion <= DateTime.Now);
        }

        [Fact]
        public void UnClienteNuevo_TieneLosTextosVaciosYNoNulos()
        {
            var cliente = new ClienteDesktop();

            Assert.Equal("", cliente.Dni);
            Assert.Equal("", cliente.Email);
        }
    }
}