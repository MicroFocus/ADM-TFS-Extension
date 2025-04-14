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
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace PSModule.Common
{
    public class Aes256Encrypter
    {
        private readonly byte[] _keyBytes;
        private const int INT_256 = 256;
        private const int INT_128 = 128;

        public Aes256Encrypter(SecureString privateKey)
        {
            string secretKey = privateKey?.AsPlainText();
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("The secret key is null or empty.", nameof(secretKey));
            }

            _keyBytes = Encoding.UTF8.GetBytes(secretKey);

            if (_keyBytes.Length != 32)
            {
                throw new ArgumentException("The secret key must be 32 bytes (256 bits) when UTF-8 encoded.", nameof(secretKey));
            }
        }

        public string Decrypt(string text)
        {
            if (text.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(text));

            byte[] encryptedBytes;
            try
            {
                encryptedBytes = Convert.FromBase64String(text);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("The input text is not a valid Base64 string.", nameof(text), ex);
            }

            if (encryptedBytes.Length < 16)
                throw new ArgumentException("The input data is too short to contain an IV.", nameof(text));

            using Aes aes = Aes.Create();
            aes.KeySize = INT_256;
            aes.BlockSize = INT_128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keyBytes;

            // Extract IV from the first 16 bytes
            byte[] iv = new byte[16];
            Array.Copy(encryptedBytes, 0, iv, 0, 16);
            aes.IV = iv;

            // Extract ciphertext (skip IV)
            byte[] ciphertext = new byte[encryptedBytes.Length - 16];
            Array.Copy(encryptedBytes, 16, ciphertext, 0, ciphertext.Length);

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            try
            {
                using MemoryStream msDecrypt = new(ciphertext);
                using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new(csDecrypt);
                return srDecrypt.ReadToEnd();
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException("Decryption failed. The input may not be valid encrypted data, or the key may not match.", ex);
            }
        }

        public string Encrypt(string text)
        {
            using Aes aes = Aes.Create();
            aes.KeySize = INT_256;
            aes.BlockSize = INT_128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keyBytes;
            aes.GenerateIV(); // Random IV

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using MemoryStream msEncrypt = new();
            // Write IV to the start of the output
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);
            using CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write);
            using StreamWriter swEncrypt = new(csEncrypt);
            swEncrypt.Write(text);
            swEncrypt.Flush();
            csEncrypt.FlushFinalBlock();
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
    }
}