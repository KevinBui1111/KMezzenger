using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace KMezzenger.Helper
{
    public static class Password
    {
        static readonly Random rd = new Random();
        const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ123456789";

        public static string CreateRandomPassword(int passwordLength = 10)
        {
            char[] chars = new char[passwordLength];

            for (int i = 0; i < passwordLength; ++i)
                chars[i] = allowedChars[rd.Next() % allowedChars.Length];

            return new string(chars);
        }

        public static string GenerateSalt()
        {
            //Generate a cryptographic random number. 
            byte[] data = new byte[0x10];
            new RNGCryptoServiceProvider().GetBytes(data);

            // Return a Base64 string representation of the random number. 
            return Convert.ToBase64String(data);
        }

        public static string EncodePassword(string password, string salt)
        {
            HashAlgorithm algorithm = new SHA1Managed();

            byte[] passByte = Encoding.UTF8.GetBytes(password);
            byte[] saltByte = Convert.FromBase64String(salt);

            byte[] buffer = new byte[passByte.Length + saltByte.Length];
            Buffer.BlockCopy(saltByte, 0, buffer, 0, saltByte.Length);
            Buffer.BlockCopy(passByte, 0, buffer, saltByte.Length, passByte.Length);

            byte[] hashedPwd = algorithm.ComputeHash(buffer);

            // Return a Base64 string representation of the encoded password. 
            return Convert.ToBase64String(hashedPwd);
        }

    }
}