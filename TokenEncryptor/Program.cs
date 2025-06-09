using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TokenEncryptor;

public static class TokenEncryptorModul
{
    public static bool checkEncryptedToken(string appSettingsPath)
    {
        if (!File.Exists(appSettingsPath))
        {
            Console.WriteLine("{appSettingsPath} is not found");
            return false;
        }

        var config = File.ReadAllText(appSettingsPath);
        var doc = JsonDocument.Parse(config);

        var root = doc.RootElement;

        if (root.TryGetProperty("ApiAuthentication", out var apiSettings) && apiSettings.TryGetProperty("Token", out _))
        {
            Console.WriteLine("Encrypted Token already exists.");
            return true;
        }

        Console.WriteLine("Enter your Bearer token: ");
        var token = Console.ReadLine();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Invalid token!");
            return false;
        }

        using var aes = Aes.Create();
        var key = aes.Key;
        var iv = aes.IV;

        string encryptedToken;
        using (var encryptor = aes.CreateEncryptor())
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(tokenBytes, 0, tokenBytes.Length);
            cs.FlushFinalBlock();
            encryptedToken = Convert.ToBase64String(ms.ToArray());
        }

        var appSettingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(config) ?? new Dictionary<string, object>();

        var conficSection = new Dictionary<string, string>
        {
            {"Token", encryptedToken},
            {"AESKey", Convert.ToBase64String(key)},
            {"AESIV", Convert.ToBase64String(iv)},
            {"BaseUrl", "https://ix-api.alwaysdata.net"}
        };

        appSettingsDict["ApiAuthentication"] = conficSection;

        var updateConfig = JsonSerializer.Serialize(appSettingsDict, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(appSettingsPath, updateConfig);

        Console.WriteLine("Token successfully encrypted and stored in {appSettingsPath}");
        return true;

    }
}