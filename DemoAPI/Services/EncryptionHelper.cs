using System;
using System.Security.Cryptography;

namespace DemoAPI.Services;

public class EncryptionHelper
{
    public static string DecryptToken(string token, string base64Key, string base64IV)
    {
        var cipher = Convert.FromBase64String(token);
        var key = Convert.FromBase64String(base64Key);
        var iv = Convert.FromBase64String(base64IV);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryption = aes.CreateDecryptor();
        using var stream = new CryptoStream(new MemoryStream(cipher), decryption, CryptoStreamMode.Read);
        return new StreamReader(stream).ReadToEnd();
    }
}
