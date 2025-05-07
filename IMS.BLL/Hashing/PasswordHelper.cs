using System.Security.Cryptography;
using System.Text;

namespace IMS.BLL.Hashing
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            if (hashedPassword == null)
                throw new ArgumentNullException(nameof(hashedPassword));
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
