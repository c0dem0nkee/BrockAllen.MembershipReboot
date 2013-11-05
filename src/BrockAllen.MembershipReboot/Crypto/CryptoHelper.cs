/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace BrockAllen.MembershipReboot
{
    public static class CryptoHelper
    {
        internal const char PasswordHashingIterationCountSeparator = '.';
        internal static Func<int> GetCurrentYear = () => DateTime.Now.Year;

        internal static string Hash(string value)
        {
            return Crypto.Hash(value);
        }
        
        internal static string Hash(string value, string key)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("key");

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            
            var alg = new System.Security.Cryptography.HMACSHA512(keyBytes);
            var hash = alg.ComputeHash(valueBytes);
            
            var result = Crypto.BinaryToHex(hash);
            return result;
        }

        internal static string GenerateNumericCode(int digits)
        {
            // 18 is good size for a long
            if (digits > 18) digits = 18;
            if (digits <= 0) digits = 6;
            
            var bytes = Crypto.GenerateSaltInternal(sizeof(long));
            var val = BitConverter.ToInt64(bytes, 0);
            var mod = (int)Math.Pow(10, digits);
            val %= mod;
            val = Math.Abs(val);
            
            return val.ToString("D" + digits);
        }

        internal static string GeneratePinPositions() {
            var values = new int[2];

            values[0] = GetRandomIntBetween(1,6);
            values[1] = values[0];
            
            while (values[0] == values[1]) {
                values[1] = GetRandomIntBetween(1,6);
            }
            
            Array.Sort(values);

            return string.Join(";", values); 
        }

        internal static int GetRandomIntBetween(int minValue, int maxValue) {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException("minValue");
            if (minValue == maxValue) return minValue;
          
            var rng = new RNGCryptoServiceProvider();
            var uint32Buffer = new byte[4];
            long diff = (long)maxValue - minValue + 1;

            while (true) {
                rng.GetBytes(uint32Buffer);
                uint rand = BitConverter.ToUInt32(uint32Buffer, 0);
                const long max = (1 + (long)int.MaxValue);
                long remainder = max % diff;
                if (rand < max - remainder) {
                    return (int)(minValue + (rand % diff));
                }
            }
        }
        
        internal static string GenerateSalt()
        {
            return Crypto.GenerateSalt();
        }

        internal static string HashPassword(string password)
        {
            var count = SecuritySettings.Instance.PasswordHashingIterationCount;
            if (count <= 0)
            {
                count = GetIterationsFromYear(GetCurrentYear());
            }
            var result = Crypto.HashPassword(password, count);
            return EncodeIterations(count) + PasswordHashingIterationCountSeparator + result;
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword.Contains(PasswordHashingIterationCountSeparator))
            {
                var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
                if (parts.Length != 2) return false;

                int count = DecodeIterations(parts[0]);
                if (count <= 0) return false;

                hashedPassword = parts[1];
                
                return Crypto.VerifyHashedPassword(hashedPassword, password, count);
            }
            else
            {
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
        }

        internal static string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        internal static int DecodeIterations(string prefix)
        {
            int val;
            if (Int32.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                return val;
            }
            return -1;
        }

        // from OWASP : https://www.owasp.org/index.php/Password_Storage_Cheat_Sheet
        const int StartYear = 2000;
        const int StartCount = 1000;
        internal static int GetIterationsFromYear(int year)
        {
            if (year > StartYear)
            {
                var diff = (year - StartYear) / 2;
                var mul = (int)Math.Pow(2, diff);
                int count = StartCount * mul;
                // if we go negative, then we wrapped (expected in year ~2044). 
                // Int32.Max is best we can do at this point
                if (count < 0) count = Int32.MaxValue;
                return count;
            }
            return StartCount;
        }
    }
}
