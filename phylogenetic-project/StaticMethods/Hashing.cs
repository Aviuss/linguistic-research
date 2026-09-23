using System;
using System.Security.Cryptography;
using System.Text;

namespace phylogenetic_project.StaticMethods;

public static class Hashing
{
    public static string Sha256Hex(byte[] data)
    {
        return Convert.ToHexString(SHA256.HashData(data));
    }

    public static string Sha256Hex(string text)
    {
        return Sha256Hex(Encoding.UTF8.GetBytes(text));
    }

    public static string HashFile(string path)
    {
        return Sha256Hex(File.ReadAllBytes(path));
    }

    public static string Combine(params (string label, string value)[] parts)
    {
        var sb = new StringBuilder();
        foreach (var (label, value) in parts)
        {
            sb.Append(label.Length).Append(':').Append(label)
              .Append(value.Length).Append(':').Append(value);
        }
        return Sha256Hex(sb.ToString());
    }
}
