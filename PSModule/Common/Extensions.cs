/*
 * MIT License https://github.com/MicroFocus/ADM-TFS-Extension/blob/master/LICENSE
 *
 * Copyright 2016-2024 Open Text
 *
 * The only warranties for products and services of Open Text and its affiliates and licensors ("Open Text") are as may be set forth in the express warranty statements accompanying such products and services.
 * Nothing herein should be construed as constituting an additional warranty.
 * Open Text shall not be liable for technical or editorial errors or omissions contained herein. 
 * The information contained herein is subject to change without notice.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PSModule.Common;
using PSModule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using C = PSModule.Common.Constants;

namespace PSModule
{
	public static class Extensions
	{
		private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver { },
			NullValueHandling = NullValueHandling.Include
		};
		private static readonly JsonSerializerSettings _jsonSerializerSettings2 = new()
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver { },
			NullValueHandling = NullValueHandling.Ignore,
		};
		public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue = default)
		{
			if (dictionary.TryGetValue(key, out V value))
			{
				return value;
			}
			return defaultValue;
		}

		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

        public static bool IsEmptyOrWhiteSpace(this string str)
		{
			return str != null && str.Trim() == string.Empty;
		}

		public static bool IsValidUrl(this string url)
		{
			return Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
		}

		public static bool EqualsIgnoreCase(this string s1, string s2)
		{
			return s1?.Equals(s2, StringComparison.OrdinalIgnoreCase) ?? (s2 == null);
		}

		public static bool In(this string str, bool ignoreCase, params string[] values)
		{
			if (ignoreCase)
			{
				return values?.Any((string s) => EqualsIgnoreCase(str, s)) ?? (str == null);
			}
			return In(str, values);
		}

        public static bool In(this string str, IList<string> values, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return values?.Any((string s) => EqualsIgnoreCase(str, s)) ?? (str == null);
            }
            return In(str, values);
        }

        public static bool In<T>(this T obj, params T[] values)
		{
			return values?.Any((T o) => Equals(obj, o)) ?? false;
		}

        public static bool In<T>(this T obj, IList<T> values)
        {
            return values?.Any((T o) => Equals(obj, o)) ?? false;
        }
        public static bool IsNullOrEmpty<T>(this T[] arr)
		{
			return arr == null || arr.Length == 0;
		}

		// ICollection is base class of IList and IDictionary
		public static bool IsNullOrEmpty<T>(this ICollection<T> coll)
		{
			return coll == null || coll.Count == 0;
		}

		public static bool IsNull(this DateTime dt)
		{
			if (Convert.GetTypeCode(dt) != 0 && (dt.Date != DateTime.MinValue.Date))
			{
				return dt.Date == DateTime.MaxValue.Date;
			}
			return true;
		}

		public static bool IsNullOrEmpty(this DateTime? dt)
		{
			if (dt.HasValue)
			{
				return IsNull(dt.Value);
			}
			return true;
		}

		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (T item in enumeration)
			{
				action(item);
			}
		}
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T, int> action)
        {
			int x = 0;
            foreach (T item in enumeration)
            {
                action(item, ++x);
            }
        }
        public static string GetMD5Hash(this string text)
		{
			using MD5 md5 = MD5.Create();
			byte[] computedHash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
			return new SoapHexBinary(computedHash).ToString();
		}

		public static string AppendSuffix(this Uri uri, string suffix)
		{
			try
			{
                UriBuilder uriBuilder = new(uri);
				uriBuilder.Path = Path.Combine(uriBuilder.Path, suffix);
				return Uri.UnescapeDataString(uriBuilder.ToString());
			}
			catch
			{
				string prefix = uri.ToString().TrimEnd(C.SLASH);
				suffix = suffix.TrimStart(C.SLASH);
				return $"{prefix}/{suffix}";
			}
		}
		public static T DeserializeXML<T>(this string xml) where T : class
		{
            XmlSerializer ser = new(typeof(T));
			using StringReader sr = new(xml);
			return (T)ser.Deserialize(sr);
		}
		public static string GetStringValue(this Enum value)
		{
			string stringValue = value.ToString();
			Type type = value.GetType();
			FieldInfo fieldInfo = type.GetField(value.ToString());
			if (fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) is StringValueAttribute[] attrs && attrs.Any())
			{
				stringValue = attrs[0].Value;
			}
			return stringValue;
		}
		public static string ToXML<T>(this T obj) where T : class
		{
			string result = null;
			if (obj != null)
			{
				XmlSerializer serializer = new (obj.GetType());
                using MemoryStream ms = new ();
                serializer.Serialize(ms, obj);
                ms.Position = 0;
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

			return result;
		}

		public static string ToJson<T>(this T obj, bool indented = true, bool escapeDblQuotes = false, bool ignoreNullValues = false) where T : class
        {
			string json = JsonConvert.SerializeObject(obj,
                indented ? Formatting.Indented : Formatting.None,
                ignoreNullValues ? _jsonSerializerSettings2 : _jsonSerializerSettings);
            return escapeDblQuotes ? json.EscapeDblQuotes() : json;
		}

        public static string ToJson<T>(this T obj, char[] escapeChars, bool indented = false, bool ignoreNullValues = false) where T : class
        {
            string json = JsonConvert.SerializeObject(obj, 
				indented ? Formatting.Indented : Formatting.None, 
				ignoreNullValues ? _jsonSerializerSettings2 : _jsonSerializerSettings);
            return json.Escape(escapeChars);
        }
        public static T FromJson<T>(this string json, JsonSerializerSettings jsonSerializerSettings = null) where T : class
        {
			return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings ?? _jsonSerializerSettings);
		}

		public static bool StartsWithAny(this string str, params char[] chars)
		{
			return !string.IsNullOrEmpty(str) && chars.Any(c => c == str[0]);
		}

		public static bool StartsWithAny(this string str, params string[] strs)
		{
			return !string.IsNullOrEmpty(str) && strs.Any(str.StartsWith);
		}

		public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
		{
			return source.Select((item, index) => (item, index));
		}

		public static string Escape(this string json, char[] escapeChars)
		{
            StringBuilder output = new(json.Length);
			foreach (char c in json)
			{
                output.Append(escapeChars.Contains(c) ? @$"\{c}" : $"{c}");
			}

			return output.ToString();
		}

        public static string EscapeDblQuotes(this string json)
        {
			if (json == null) return null;
            StringBuilder output = new(json.Length);
            foreach (char c in json)
            {
                output.Append(c == C.DBL_QUOTE_ ? @$"\{c}" : $"{c}");
            }

            return output.ToString();
        }

        public static bool AreTestRunsFromJsonRpt(this ReportMetaData testCase)
        {
            return testCase?.TestRuns?.Any(tr => tr.Id == 0) == true;
        }

        /// <summary>
        /// Converts a SecureString to plain text. Warning: This exposes secure data in memory.
        /// </summary>
        /// <param name="secretValue">The SecureString to convert</param>
        /// <returns>The plain text string</returns>
        /// <exception cref="ArgumentNullException">Thrown if secretValue is null</exception>
        public static string AsPlainText(this SecureString secretValue)
        {
            if (secretValue == null)
                throw new ArgumentNullException(nameof(secretValue));

            if (secretValue.Length == 0)
                return string.Empty;

            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(secretValue);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                if (valuePtr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
        }

        public static SecureString ToSecureString(this string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return new SecureString();

            SecureString secureString = new();
            foreach (char c in plainText)
            {
                secureString.AppendChar(c);
            }
            secureString.MakeReadOnly();
            return secureString;
        }

        public static SecureString ToSecureString(this byte[] bytes)
        {
            string str = Encoding.Unicode.GetString(bytes);
            SecureString secureString = new();

            foreach (char c in str)
            {
                secureString.AppendChar(c);
            }

            // Make the SecureString read-only and return it
            secureString.MakeReadOnly();
            return secureString;
        }

        public static byte[] ToByteArray(this SecureString secureString)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(secureString);
            try
            {
                // Read the length prefix of the BSTR (4 bytes before the pointer)
                int length = Marshal.ReadInt32(bstr, -4);
                byte[] bytes = new byte[length];
                Marshal.Copy(bstr, bytes, 0, length);
                return bytes;
            }
            finally
            {
                // Free the BSTR to prevent memory leaks
                Marshal.ZeroFreeBSTR(bstr);
            }
        }
    }
}