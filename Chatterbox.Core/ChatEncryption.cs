using System;
using System.Security.Cryptography;
using System.Text;

namespace Chatterbox.Core;

public static class ChatEncryption
{

    internal const string Key = "Chatterbox";

    private static string FixKeyLength(string key)
    {
        if (string.IsNullOrEmpty(key) || key.Length > 16)
            throw new ArgumentOutOfRangeException(nameof(key));
        if (key.Length == 16)
            return key;
        var charactersLeft = 16 - key.Length;
        for (var index = 0; index < charactersLeft; index++)
            key += " ";
        return key;
    }

    public static string EncryptData(string data, string key)
    {
        key = FixKeyLength(key);
        var bytes = Encoding.UTF8.GetBytes(data);
        var provider = new AesCryptoServiceProvider
        {
            Key = Encoding.UTF8.GetBytes(key),
            Mode = CipherMode.ECB,
            Padding = PaddingMode.PKCS7
        };
        var encryptor = provider.CreateEncryptor();
        var result = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
        provider.Clear();
        return Convert.ToBase64String(result, 0, result.Length);
    }

    public static bool DecryptData(string data, string key, out string? output)
    {
        key = FixKeyLength(key);
        try
        {
            var bytes = Convert.FromBase64String(data);
            var provider = new AesCryptoServiceProvider
            {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var decryptor = provider.CreateDecryptor();
            var resultArray = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);
            provider.Clear();
            output = Encoding.UTF8.GetString(resultArray);
            return true;
        }
        catch
        {
            output = null;
            return false;
        }
    }

}