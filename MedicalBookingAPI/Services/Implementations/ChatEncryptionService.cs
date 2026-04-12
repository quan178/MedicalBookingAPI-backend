using System.Security.Cryptography;
using System.Text;
using MedicalBookingAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MedicalBookingAPI.Services.Implementations;

public class ChatEncryptionService : IChatEncryptionService
{
    private readonly byte[] _masterKey;

    public ChatEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption key not configured");
        _masterKey = Convert.FromBase64String(keyBase64);

        if (_masterKey.Length != 32)
            throw new ArgumentException("Encryption key must be 32 bytes (256 bits) for AES-256");
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _masterKey;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return $"{Convert.ToBase64String(aes.IV)}.{Convert.ToBase64String(cipherBytes)}";
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var parts = cipherText.Split('.');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid cipher text format");

        var iv = Convert.FromBase64String(parts[0]);
        var cipherBytes = Convert.FromBase64String(parts[1]);

        using var aes = Aes.Create();
        aes.Key = _masterKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
