using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Students.Web.Models
{
    public static class Utility
    {
        public static string GetHash(string password)
        {
            using var derivedBytes = new Rfc2898DeriveBytes(password, saltSize: 16, iterations: 5000, HashAlgorithmName.SHA512);
            var salt = derivedBytes.Salt;
            byte[] key = derivedBytes.GetBytes(16); // 128 bits key
            return Convert.ToBase64String(key);
        }

        public static string GetPassword(string login)
        {
            var _random = new Random();
            char[] low = Enumerable.Range('a', 'z' - 'a' + 1).Select(i => (char)i).ToArray();
            char[] upper = low.Select(char.ToUpper).ToArray();
            int[] numbers = Enumerable.Range(1, 9).ToArray();
            const int M = 12;
            int N = login.Length;
            var first = (int)(Math.Pow(N, 3) % 5) + 1;
            var second = (int)(Math.Pow(N, 2) % 6) + 1 + first;
            var sb = new StringBuilder();

            for (int i = 1; i <= first; i++)
            {
                sb = sb.Append(low[_random.Next(0, low.Length)]);
            }

            for (int i = first + 1; i <= second; i++)
            {
                sb = sb.Append(upper[_random.Next(0, upper.Length)]);
            }

            for (int i = second + 1; i < M; i++)
            {
                sb = sb.Append(numbers[_random.Next(0, numbers.Length)]);
            }

            if (login.Length > 1 + (2d / 3d) * (double)M)
            {
                return null;
            }

            return string.Concat(sb.ToString().Take(M));
        }
    }
}
