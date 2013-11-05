using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace BrockAllen.MembershipReboot {

    internal class Authenticator {
        // Based on the code at http://stackoverflow.com/a/12398317/465404
        const int IntervalLength = 30;
        const int PinLength = 6;
        static readonly int PinModulo = (int)Math.Pow(10, PinLength);
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static byte[] GenerateSecretKey() {
            var secretKey = new byte[10];

            using (var rng = RandomNumberGenerator.Create()) {
                rng.GetBytes(secretKey);
            }

            return secretKey;
        }

        /// <summary>
        /// Number of intervals that have elapsed.
        /// </summary>
        static long CurrentInterval {
            get {
                var elapsedSeconds = (long)Math.Floor((DateTime.UtcNow - UnixEpoch).TotalSeconds);

                return elapsedSeconds / IntervalLength;
            }
        }

        /// <summary>
        /// Generates a pin for the given key.
        /// </summary>
        public string GeneratePin(byte[] key) {
            return GeneratePin(key, CurrentInterval);
        }

        /// <summary>
        /// Generates a pin by hashing a key and counter.
        /// </summary>
        static string GeneratePin(byte[] key, long counter) {
            const int sizeOfInt32 = 4;

            var counterBytes = BitConverter.GetBytes(counter);

            if (BitConverter.IsLittleEndian) {
                //spec requires bytes in big-endian order
                Array.Reverse(counterBytes);
            }

            var hash = new HMACSHA1(key).ComputeHash(counterBytes);
            var offset = hash[hash.Length - 1] & 0xF;

            var selectedBytes = new byte[sizeOfInt32];
            Buffer.BlockCopy(hash, offset, selectedBytes, 0, sizeOfInt32);

            if (BitConverter.IsLittleEndian) {
                //spec interprets bytes in big-endian order
                Array.Reverse(selectedBytes);
            }

            var selectedInteger = BitConverter.ToInt32(selectedBytes, 0);

            //remove the most significant bit for interoperability per spec
            var truncatedHash = selectedInteger & 0x7FFFFFFF;

            //generate number of digits for given pin length
            var pin = truncatedHash % PinModulo;

            return pin.ToString(CultureInfo.InvariantCulture).PadLeft(PinLength, '0');
        }
    }
}
