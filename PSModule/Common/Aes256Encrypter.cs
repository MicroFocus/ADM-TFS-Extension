/*
 * Certain versions of software accessible here may contain branding from Hewlett-Packard Company (now HP Inc.) and Hewlett Packard Enterprise Company.
 * This software was acquired by Micro Focus on September 1, 2017, and is now offered by OpenText.
 * Any reference to the HP and Hewlett Packard Enterprise/HPE marks is historical in nature, and the HP and Hewlett Packard Enterprise/HPE marks are the property of their respective owners.
 * __________________________________________________________________
 * MIT License
 *
 * Copyright 2012-2025 Open Text
 *
 * The only warranties for products and services of Open Text and
 * its affiliates and licensors ("Open Text") are as may be set forth
 * in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or
 * omissions contained herein. The information contained herein is subject
 * to change without notice.
 *
 * Except as specifically indicated otherwise, this document contains
 * confidential information and a valid license is required for possession,
 * use or copying. If this work is provided to the U.S. Government,
 * consistent with FAR 12.211 and 12.212, Commercial Computer Software,
 * Computer Software Documentation, and Technical Data for Commercial Items are
 * licensed to the U.S. Government under vendor's standard commercial license.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ___________________________________________________________________
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace PSModule.Common
{
    public class Aes256Encrypter
    {
        // =========================================================
        // ATTRIBUTES:
        // =========================================================
        // Singleton instance — null until Create() is called from Main.
        private static Aes256Encrypter _instance;

        // Per-instance secure keys set once in the private constructor.
        private readonly byte[] _aesKey;
        private readonly byte[] _hmacKey;
        private readonly byte[] _privateKey;

        // =========================================================
        // SINGLETON FACTORY
        // =========================================================

        /// <summary>
        /// Creates the singleton. Must be called once from Main, before any
        /// Encrypt / Decrypt call, passing the raw byte-array key.
        /// </summary>
        public static void Create(byte[] privateKey = null)
        {
            if (_instance != null)
                throw new InvalidOperationException("Encrypter is already initialized.");
            if (privateKey.IsNullOrEmpty())
            {
                privateKey = new byte[64];
                using RandomNumberGenerator rng = RandomNumberGenerator.Create();
                rng.GetBytes(privateKey);
            }
            _instance = new Aes256Encrypter(privateKey);
        }

        /// <summary>
        /// Splits the 64-byte key into two 32-byte halves: _aesKey (for AES-256) and _hmacKey (for HMAC-SHA256).
        /// If no key is provided, an ArgumentNullException is thrown.
        /// </summary>
        /// <param name="privateKey">The 64-byte key.</param>
        /// <exception cref="ArgumentNullException">Thrown if the key is null or empty.</exception>
        /// <exception cref="CryptographicException">Thrown if the key is invalid.</exception>
        private Aes256Encrypter(byte[] privateKey)
        {
            if (privateKey is null)
                throw new ArgumentNullException(nameof(privateKey));

            if (privateKey.Length != 64)
                throw new CryptographicException("Invalid secure key length. Expected 64 bytes (base64-encoded).");

            _aesKey = new byte[32];
            _hmacKey = new byte[32];
            _privateKey = privateKey;
            Buffer.BlockCopy(privateKey, 0, _aesKey, 0, 32);
            Buffer.BlockCopy(privateKey, 32, _hmacKey, 0, 32);
        }

        // =========================================================
        // 🔒 PUBLIC STATIC API (callers are unchanged)
        // =========================================================
        /// <summary>
        /// Entry point for encryption.
        /// Delegates to EncryptSecure, using Singleton instance.
        /// <param name="plainText">The plaintext to encrypt.</param>
        /// <returns>The encrypted ciphertext.</returns>
        /// </summary>
        public static string Encrypt(string plainText) =>
            _instance?.EncryptSecure(plainText);

        /// <summary>
        /// Entry point for decryption.
        /// In DEBUG mode returns the input as-is.
        /// Otherwise delegates to DecryptSecure, using Singleton instance.
        /// </summary>
        /// <param name="cipherText">The ciphertext to decrypt.</param>
        /// <returns>The decrypted plaintext.</returns>
        public static string Decrypt(string cipherText)
        {
#if DEBUG
            return cipherText; // used for troubleshooting and testing without needing to set up keys
#endif
            if (cipherText.IsNullOrEmpty())
                return cipherText;

            return _instance?.DecryptSecure(cipherText);
        }

        // =========================================================
        // 🔐 SECURE MODE (AES-256-CBC + HMAC)
        // =========================================================
        /// <summary>
        /// Encrypts using AES-256-CBC with a randomly generated IV, then appends an HMAC-SHA256 tag.
        /// Output layout: [ IV (16) | ciphertext | HMAC (32) ], base64-encoded.
        /// </summary>
        /// <param name="plainText">The plaintext to encrypt.</param>
        /// <returns>The encrypted ciphertext.</returns>
        private string EncryptSecure(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = _aesKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            byte[] iv = aes.IV; // 16 bytes

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Layout: [ IV (16) | ciphertext | HMAC (32) ]
            byte[] data = new byte[16 + ciphertext.Length];
            Buffer.BlockCopy(iv, 0, data, 0, 16);
            Buffer.BlockCopy(ciphertext, 0, data, 16, ciphertext.Length);

            using HMACSHA256 h = new(_hmacKey);
            byte[] hmac = h.ComputeHash(data);

            byte[] result = new byte[data.Length + 32];
            Buffer.BlockCopy(data, 0, result, 0, data.Length);
            Buffer.BlockCopy(hmac, 0, result, data.Length, 32);

            return Convert.ToBase64String(result);
        }

        /// <summary>
        /// Decodes the base64 payload, verifies the HMAC using constant-time comparison,
        /// then decrypts the ciphertext with AES-256-CBC using the embedded IV.
        /// </summary>
        /// <param name="encryptedText">The base64-encoded encrypted payload.</param>
        /// <returns>The decrypted plaintext.</returns>
        /// <exception cref="CryptographicException">Thrown if the payload is invalid or HMAC validation fails.</exception>
        private string DecryptSecure(string encryptedText)
        {
            byte[] buffer = Convert.FromBase64String(encryptedText);
            // minimum: 16 (IV) + 1 block (16) + 32 (HMAC) = 64
            if (buffer.Length < 64)
                throw new CryptographicException("Invalid encrypted payload.");

            int ciphertextLen = buffer.Length - 16 - 32;

            byte[] iv = new byte[16];
            byte[] ciphertext = new byte[ciphertextLen];
            byte[] hmac = new byte[32];

            Buffer.BlockCopy(buffer, 0, iv, 0, 16);
            Buffer.BlockCopy(buffer, 16, ciphertext, 0, ciphertextLen);
            Buffer.BlockCopy(buffer, buffer.Length - 32, hmac, 0, 32);

            using HMACSHA256 h = new HMACSHA256(_hmacKey);
            byte[] expected = h.ComputeHash(buffer, 0, buffer.Length - 32);

            if (!ConstantTimeEquals(expected, hmac))
                throw new CryptographicException("HMAC validation failed.");

            using Aes aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plain = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);

            return Encoding.UTF8.GetString(plain);
        }

        // =========================================================
        // 🛠 HELPERS
        // =========================================================
        /// <summary>
        /// Compares two byte arrays in constant time (bitwise OR of all differences)
        /// to prevent timing-based side-channel attacks during HMAC verification.
        /// </summary>
        /// <param name="a">The first byte array to compare.</param>
        /// <param name="b">The second byte array to compare.</param>
        /// <returns>True if the byte arrays are equal, false otherwise.</returns>
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }

        /// <summary>
        /// Returns the private key of the current singleton instance, or <c>null</c> if not initialized.
        /// </summary>
        public static byte[] GetPrivateKey() => _instance?._privateKey;
    }
}
