using EAI.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace EAI.Infrastructure.Services;

public class EncryptionService : IEncryptionService
{
    private readonly ILogger<EncryptionService> _logger;
    private readonly string _encryptionKey;

    public EncryptionService(ILogger<EncryptionService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _encryptionKey = configuration["Security:EncryptionKey"] ?? "default-encryption-key-32-chars";
    }

    public async Task<string> EncryptAsync(string data)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);

            await swEncrypt.WriteAsync(data);
            await swEncrypt.FlushAsync();
            csEncrypt.FlushFinalBlock();

            var encrypted = msEncrypt.ToArray();
            var result = Convert.ToBase64String(aes.IV.Concat(encrypted).ToArray());
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting data");
            throw;
        }
    }

    public async Task<string> DecryptAsync(string encryptedData)
    {
        try
        {
            var fullCipher = Convert.FromBase64String(encryptedData);
            var iv = fullCipher.Take(16).ToArray();
            var cipher = fullCipher.Skip(16).ToArray();

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipher);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return await srDecrypt.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting data");
            throw;
        }
    }
}
