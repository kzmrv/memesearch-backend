using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MemeScrapper
{
    public static class DirectoryExtensions
    {
        static readonly JsonSerializerSettings settings;

        static DirectoryExtensions()
        {
            settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                //DateFormatString = "dd-MM-yy HH:mm",
            };

            settings.Converters.Add(new StringEnumConverter(true));
        }

        public static string GetJson(object data, Formatting formatting = Formatting.None)
        {
            if (data == null)
                return null;
            return JsonConvert.SerializeObject(data, formatting, settings);
        }

        public static string TempPath => "c:\\temp\\";
        public static void SaveJson(object data) => SaveJson($"{TempPath}temp.json", data);

        public static void SaveJson(string path, object data) =>
            File.WriteAllText(path, GetJson(data, Formatting.Indented));

        public static void SaveText(string text) => File.WriteAllText($"{TempPath}temp.txt", text);

        public static T GetJson<T>(string path) => JsonConvert.DeserializeObject<T>(File.ReadAllText(path));

        public static string TrimEnd(this string x, string trim) =>
            x?.EndsWith(trim) == true ? x.Substring(0, x.Length - trim.Length) : x;

        public static string TrimStart(this string x, string trim) =>
            x?.StartsWith(trim) == true ? x.Substring(trim.Length) : x;

    }

    public static class Extensions
    {
        public static IEnumerable<T> Distinct<T, TIdentity>(this IEnumerable<T> source, Func<T, TIdentity> identitySelector)
        {
            return source.Distinct(new DelegateEqualityComparer<T, TIdentity>(identitySelector));
        }

        class DelegateEqualityComparer<T, TIdentity> : IEqualityComparer<T>
        {
            readonly Func<T, TIdentity> identitySelector;

            public DelegateEqualityComparer(Func<T, TIdentity> identitySelector)
            {
                this.identitySelector = identitySelector;
            }

            public bool Equals(T x, T y)
            {
                return Equals(identitySelector(x), identitySelector(y));
            }

            public int GetHashCode(T obj)
            {
                return identitySelector(obj).GetHashCode();
            }
        }
    }

    public static class HashingExtensions
    {
        public static string Md5Hash(byte[] bytes)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var hash = Convert.ToBase64String(sha1.ComputeHash(bytes));
                return hash;
            }
        }

        public static string Md5Hash(string key)
        {
            var bytes = Encoding.UTF8.GetBytes(key);
            var hash = Md5Hash(bytes);
            return hash;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
    }
}
