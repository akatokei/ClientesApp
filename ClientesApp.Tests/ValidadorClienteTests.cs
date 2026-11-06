using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ClientesApp.Models;
using ClientesApp.Services;
using Xunit;

namespace ClientesApp.Tests
{
    public class ValidadorClienteTests
    {
        // Un cliente 100% correcto. En cada test estropeamos UN campo.
        private static ClienteDesktop ClienteValido()
        {
            return new ClienteDesktop
            {
                Dni = "12345678Z",
                Nombre = "Ana",
                Apellidos = "Pérez Gil",
                FechaNacimiento = "23/05/1990",
                Telefono = "600123456",
                Email = "ana@gmail.com"
            };
        }

        [Fact]
        public void ClienteCorrecto_NoTieneErrores()
        {
            Assert.Empty(ValidadorCliente.TodosLosErrores(ClienteValido()));
            Assert.False(ValidadorCliente.TieneErrores(ClienteValido()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void DniVacio_SeMarcaComoError(string dni)
        {
            var c = ClienteValido();
            c.Dni = dni;

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Dni"));
        }

        [Fact]
        public void DniConLetraIncorrecta_SeMarcaComoError()
        {
            var c = ClienteValido();
            c.Dni = "12345678A";

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Dni"));
        }

        [Fact]
        public void NieCorrecto_NoEsError()
        {
            var c = ClienteValido();
            c.Dni = "X1234567L";

            Assert.Null(ValidadorCliente.ErrorDe(c, "Dni"));
        }

        [Fact]
        public void NombreVacio_SeMarcaComoError()
        {
            var c = ClienteValido();
            c.Nombre = "";

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Nombre"));
        }

        [Fact]
        public void ApellidosVacios_SeMarcanComoError()
        {
            var c = ClienteValido();
            c.Apellidos = "";

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Apellidos"));
        }

        [Fact]
        public void LosCamposOpcionalesVacios_NoSonError()
        {
            var c = ClienteValido();
            c.FechaNacimiento = "";
            c.Telefono = "";
            c.Email = "";

            Assert.Empty(ValidadorCliente.TodosLosErrores(c));
        }

        [Theory]
        [InlineData("1990-05-23")]
        [InlineData("30/02/1990")]
        [InlineData("hola")]
        public void FechaMal_SeMarcaComoError(string fecha)
        {
            var c = ClienteValido();
            c.FechaNacimiento = fecha;

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "FechaNacimiento"));
        }

        [Theory]
        [InlineData("123")]
        [InlineData("12345678")]
        [InlineData("abcdefghi")]
        public void TelefonoMal_SeMarcaComoError(string telefono)
        {
            var c = ClienteValido();
            c.Telefono = telefono;

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Telefono"));
        }

        [Theory]
        [InlineData("sinarroba")]
        [InlineData("ana@correo")]
        [InlineData("@correo.com")]
        public void EmailMal_SeMarcaComoError(string email)
        {
            var c = ClienteValido();
            c.Email = email;

            Assert.NotNull(ValidadorCliente.ErrorDe(c, "Email"));
        }

        [Fact]
        public void ElErrorDevueltoEsElEjemploCorrecto()
        {
            // Es lo que la ventana escribe en rojo
            var c = ClienteValido();
            c.Email = "mal";
            c.FechaNacimiento = "mal";

            Assert.Equal("nombre@gmail.com", ValidadorCliente.ErrorDe(c, "Email"));
            Assert.Equal("25/12/1990", ValidadorCliente.ErrorDe(c, "FechaNacimiento"));
        }

        [Fact]
        public void VariosCamposMal_SeDevuelvenTodosAlaVez()
        {
            var c = ClienteValido();
            c.Dni = "";
            c.Email = "mal";
            c.Telefono = "1";

            Dictionary<string, string> errores = ValidadorCliente.TodosLosErrores(c);

            Assert.Equal(3, errores.Count);
            Assert.True(errores.ContainsKey("Dni"));
            Assert.True(errores.ContainsKey("Email"));
            Assert.True(errores.ContainsKey("Telefono"));
        }
    }
}