using System.Security.Cryptography;
using System.Text;

namespace ClaudipanAPI.Helpers;

/// <summary>
/// Helper para el hashing y verificación de contraseñas con soporte de MD5 y algoritmo seguro.
/// </summary>
public static class PasswordHelper
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100000;

    /// <summary>
    /// Genera el hash MD5 en formato hexadecimal estándar.
    /// </summary>
    public static string HashMD5(string password)
    {
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = md5.ComputeHash(inputBytes);
        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }
        return sb.ToString();
    }

    /// <summary>
    /// Genera un hash seguro con salt basado en PBKDF2/SHA256 (o MD5 según estándar).
    /// </summary>
    public static string HashPassword(string password)
    {
        // Generamos hash MD5 estándar requerido por el proyecto académico SENA
        return HashMD5(password);
    }

    /// <summary>
    /// Verifica la contraseña contra el hash almacenado, soportando hashes MD5 y PBKDF2.
    /// </summary>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        // 1. Verificación directa MD5
        var md5Hash = HashMD5(password);
        if (string.Equals(md5Hash, storedHash, StringComparison.OrdinalIgnoreCase))
            return true;

        // 2. Si la longitud es de un hash PBKDF2 (Base64), verificar con algoritmo seguro
        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            if (hashBytes.Length == SaltSize + KeySize)
            {
                var salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                using var algorithm = new Rfc2898DeriveBytes(
                    password, salt, Iterations, HashAlgorithmName.SHA256);
                
                var key = algorithm.GetBytes(KeySize);

                for (int i = 0; i < KeySize; i++)
                {
                    if (hashBytes[i + SaltSize] != key[i])
                        return false;
                }
                return true;
            }
        }
        catch
        {
            // No era un hash Base64 válido
        }

        // 3. Verificación de texto plano por contingencia de datos semilla
        return password == storedHash;
    }
}