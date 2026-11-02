using System;
using System.IO;
using System.Reflection;

namespace LongShadowInner
{
    internal static class ShaderResourceLoader
    {
        public static byte[] GetShaderResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            // リソース名の検索（フォルダ階層の差異を吸収）
            var resourceName = Array.Find(assembly.GetManifestResourceNames(), n => n.EndsWith(name, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(resourceName))
            {
                throw new FileNotFoundException($"Embedded Resource '{name}' not found in assembly '{assembly.FullName}'.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            var bytes = new byte[stream.Length];
            stream.ReadExactly(bytes);
            return bytes;
        }
    }
}
