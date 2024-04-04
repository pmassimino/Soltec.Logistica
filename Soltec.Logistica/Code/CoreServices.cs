using System.Text.RegularExpressions;

namespace Soltec.Logistica.Code
{
    namespace BackEnd.Services.Core
    {
        public interface ICoreServices
        {
            bool ValidarNumeroDocumento(string tipo, long numero);
        }

        public static class CoreServices
        {
            #region Validacion
            public static bool IsValidCuit(string tipo, long numero)
            {
                bool result = false;
                if (tipo == "80" || tipo == "86")
                {
                    result = ValidaCuit(numero.ToString());
                }
                else
                {
                    if (numero > 0)
                    {
                        result = true;
                    }
                }
                return result;
            }
            public static bool IsValidEmail(object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return true;

                string email = value.ToString();
                // Expresión regular para validar la estructura de un correo electrónico
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

                return Regex.IsMatch(email, pattern);
            }

            public static bool ValidaCuit(string cuit)
            {
                if (cuit == null)
                {
                    return false;
                }

                //Quito los guiones, el cuit resultante debe tener 11 caracteres.
                cuit = cuit.Replace("-", string.Empty);
                if (cuit.Length != 11)
                {
                    return false;
                }
                else
                {
                    int calculado = CalcularDigitoCuit(cuit);
                    int digito = int.Parse(cuit.Substring(10));
                    return calculado == digito;
                }
            }

            private static int CalcularDigitoCuit(string cuit)
            {
                int[] mult = new[] { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                char[] nums = cuit.ToCharArray();
                int total = 0;
                for (int i = 0; i < mult.Length; i++)
                {
                    total += int.Parse(nums[i].ToString()) * mult[i];
                }
                var resto = total % 11;
                return resto == 0 ? 0 : resto == 1 ? 9 : 11 - resto;
            }
            #endregion
            #region Afip

            #endregion
        }
    }


}
