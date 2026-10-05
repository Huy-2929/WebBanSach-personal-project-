using System.Security.Cryptography;

namespace WebBanHang.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithm,
                KeySize
            );

            // Format: salt:iterations:hash
            return $"{Convert.ToBase64String(salt)}:{Iterations}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string? hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            // Compatibility with initial seed database value
            if (hashedPassword == "TEMP_PASSWORD_HASH")
            {
                // Accept commonly used dev passwords for seed accounts
                return providedPassword == "admin123" || providedPassword == "123456" || providedPassword == "Admin@123";
            }

            try
            {
                var parts = hashedPassword.Split(':');
                if (parts.Length != 3)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                int iterations = int.Parse(parts[1]);
                byte[] expectedHash = Convert.FromBase64String(parts[2]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    providedPassword,
                    salt,
                    iterations,
                    HashAlgorithm,
                    expectedHash.Length
                );

                return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
