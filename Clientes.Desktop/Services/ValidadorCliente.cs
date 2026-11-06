using ClientesApp.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ClientesApp.Services
{
    // Esta clase NO tiene reglas propias: solo pregunta a .NET (DataAnnotations)
    // qué campos de un cliente incumplen los atributos puestos en ClienteDesktop.
    // Nunca bloquea nada: solo informa de qué está mal.
    public static class ValidadorCliente
    {
        // Mira UN campo (ej: "Email").
        // Devuelve null si está bien, o el EJEMPLO CORRECTO si está mal.
        public static string ErrorDe(ClienteDesktop cliente, string propiedad)
        {
            // Leemos el valor actual de esa propiedad
            object valor = typeof(ClienteDesktop).GetProperty(propiedad).GetValue(cliente, null);

            // El "contexto" le dice a .NET de qué propiedad estamos hablando
            var contexto = new ValidationContext(cliente) { MemberName = propiedad };
            var resultados = new List<ValidationResult>();

            bool correcto = Validator.TryValidateProperty(valor, contexto, resultados);

            return correcto ? null : resultados[0].ErrorMessage;
        }

        // Mira TODOS los campos. Devuelve: nombre de propiedad -> ejemplo correcto.
        // Si el diccionario está vacío, el cliente está perfecto.
        public static Dictionary<string, string> TodosLosErrores(ClienteDesktop cliente)
        {
            var resultados = new List<ValidationResult>();

            // true = valida todas las propiedades, no solo las [Required]
            Validator.TryValidateObject(cliente, new ValidationContext(cliente), resultados, true);

            var errores = new Dictionary<string, string>();
            foreach (ValidationResult resultado in resultados)
            {
                foreach (string campo in resultado.MemberNames)
                {
                    if (!errores.ContainsKey(campo))
                        errores[campo] = resultado.ErrorMessage;
                }
            }
            return errores;
        }

        // ¿Tiene este cliente BIEN todos los campos obligatorios (los marcados con [Required])?
        // Se usa al cerrar: si devuelve false, el cliente no se guarda.
        // Los errores en campos opcionales NO cuentan.
        public static bool ObligatoriosCorrectos(ClienteDesktop cliente)
        {
            Dictionary<string, string> errores = TodosLosErrores(cliente);

            foreach (PropertyInfo propiedad in typeof(ClienteDesktop).GetProperties())
            {
                bool esObligatoria = propiedad.IsDefined(typeof(RequiredAttribute), false);

                if (esObligatoria && errores.ContainsKey(propiedad.Name))
                    return false;
            }

            return true;
        }

        // Método de errores
        public static bool TieneErrores(ClienteDesktop cliente)
        {
            return TodosLosErrores(cliente).Count > 0;
        }
    }
}