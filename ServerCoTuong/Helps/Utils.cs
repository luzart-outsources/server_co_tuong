using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoTuong.Helps
{
    public static class Utils
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Random số nguyên từ min đến max (bao gồm min, không bao gồm max)
        /// Nếu max <= min, trả về min.
        /// </summary>
        public static int RandomInt(int min, int max)
        {
            if (max <= min)
                return min;
            return random.Next(min, max);
        }

        /// <summary>
        /// Random số thực từ min đến max.
        /// Nếu max <= min, trả về min.
        /// </summary>
        public static double RandomDouble(double min, double max)
        {
            if (max <= min)
                return min;
            return random.NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Random true hoặc false
        /// </summary>
        public static bool RandomBool()
        {
            return random.Next(2) == 1;
        }

        /// <summary>
        /// Random 1 ký tự chữ cái thường [a-z]
        /// </summary>
        public static char RandomLowerChar()
        {
            return (char)random.Next('a', 'z' + 1);
        }

        /// <summary>
        /// Random 1 ký tự chữ cái hoa [A-Z]
        /// </summary>
        public static char RandomUpperChar()
        {
            return (char)random.Next('A', 'Z' + 1);
        }

        /// <summary>
        /// Random 1 ký tự số [0-9]
        /// </summary>
        public static char RandomDigitChar()
        {
            return (char)random.Next('0', '9' + 1);
        }

        /// <summary>
        /// Random chuỗi bất kỳ với độ dài nhất định (chữ, số)
        /// </summary>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            if (length <= 0) return "";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(chars[random.Next(chars.Length)]);
            return sb.ToString();
        }

        /// <summary>
        /// Random chuỗi chỉ có chữ thường
        /// </summary>
        public static string RandomLowerString(int length)
        {
            if (length <= 0) return "";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(RandomLowerChar());
            return sb.ToString();
        }

        /// <summary>
        /// Random chuỗi chỉ có số
        /// </summary>
        public static string RandomDigitString(int length)
        {
            if (length <= 0) return "";
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                sb.Append(RandomDigitChar());
            return sb.ToString();
        }

        /// <summary>
        /// Random ký tự bất kỳ từ một chuỗi cho sẵn
        /// </summary>
        public static char RandomCharFrom(string allowed)
        {
            if (string.IsNullOrEmpty(allowed))
                throw new ArgumentException("allowed must not be empty");
            return allowed[random.Next(allowed.Length)];
        }

        /// <summary>
        /// Random màu RGB
        /// </summary>
        public static (int r, int g, int b) RandomRGB()
        {
            return (random.Next(256), random.Next(256), random.Next(256));
        }

        public static long currentTimeMillis()
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (DateTime.UtcNow.Ticks - dateTime.Ticks) / 10000;
        }

        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public static string formatNumber(long value)
        {
            return value.ToString("N0");
        }
    }

}
