namespace WeatherApp.Helpers
{
    public static class PasswordHelper
    {
        public static byte[] HashPassword(string password)
        {
            return System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password));
        }

        public static bool VerifyPassword(string input, byte[] storedHash)
        {
            var inputHash = HashPassword(input);
            return inputHash.SequenceEqual(storedHash);
        }
    }
}
