using PSModule.AlmLabMgmtClient.SDK.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace PSModule
{
	public static class Extensions
	{
		private const char SLASH = '/';
		public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key, V defaultValue = default)
		{
			if (dictionary.TryGetValue(key, out var value))
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

		public static bool In<T>(this T obj, params T[] values)
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
		public static string GetMD5Hash(this string text)
		{
			using var md5 = MD5.Create();
			byte[] computedHash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
			return new SoapHexBinary(computedHash).ToString();
		}

		public static string AppendSuffix(this Uri uri, string suffix)
		{
			try
			{
				var uriBuilder = new UriBuilder(uri);
				uriBuilder.Path = Path.Combine(uriBuilder.Path, suffix);
				return Uri.UnescapeDataString(uriBuilder.ToString());
			}
			catch
			{
				string prefix = uri.ToString().TrimEnd(SLASH);
				suffix = suffix.TrimStart(SLASH);
				return $"{prefix}/{suffix}";
			}
		}
		public static T DeserializeXML<T>(this string xml) where T : class
		{
			var ser = new XmlSerializer(typeof(T));
			using StringReader sr = new StringReader(xml);
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
				XmlSerializer serializer = new XmlSerializer(obj.GetType());
                using MemoryStream ms = new MemoryStream();
                serializer.Serialize(ms, obj);
                ms.Position = 0;
                result = Encoding.UTF8.GetString(ms.ToArray());
            }

			return result;
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
	}
}