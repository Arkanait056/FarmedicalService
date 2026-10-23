using System.Security.Cryptography;

namespace FarmedicalService.Database
{
    ///Contraseñas con PBKDF2-SHA256 y sal aleatoria. Formato: pbkdf2$iteraciones$sal$hash.
    public static class PasswordHasher
    {
        private const string Prefijo = "pbkdf2";
        private const int Iteraciones = 100_000;

        public static string Hash(string contrasena)
        {
            var sal = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
            return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
        }

        public static bool EstaCifrada(string almacenada) => almacenada.StartsWith(Prefijo + "$", StringComparison.Ordinal);

        public static bool Verificar(string contrasena, string almacenada)
        {
            if (!EstaCifrada(almacenada))
                return contrasena == almacenada;   // contraseña antigua en texto plano (se actualiza al iniciar sesión)

            var partes = almacenada.Split('$');
            if (partes.Length != 4 || !int.TryParse(partes[1], out int iteraciones))
                return false;

            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }

        ///Mínimo 8 caracteres, con al menos una letra y un número. Devuelve null si es válida.
        public static string? ValidarReglas(string contrasena)
        {
            if (contrasena.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres.";
            if (!contrasena.Any(char.IsLetter) || !contrasena.Any(char.IsDigit))
                return "La contraseña debe combinar letras y números.";
            return null;
        }
    }
}
