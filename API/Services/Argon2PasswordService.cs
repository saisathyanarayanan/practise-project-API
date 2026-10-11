using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;

namespace API.Services;

/// <summary>
/// Enterprise Argon2id Password Hashing Service.
/// Implements OWASP recommended parameters:
/// - Algorithm: Argon2id (hybrid resistant against side-channel and GPU brute-force)
/// - Salt: 16 bytes cryptographically secure random bytes
/// - Time Cost (Iterations): 3
/// - Memory Cost: 65,536 KB (64 MB)
/// - Parallelism (Lanes): 4
/// </summary>
public class Argon2PasswordService : IArgon2PasswordService
{
    public (string HashString, string SaltBase64) HashPassword(string password)
    {
        // 1. Generate 16 bytes of cryptographically secure random salt
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string saltBase64 = Convert.ToBase64String(salt);

        // 2. Configure enterprise-grade Argon2id parameters
        var config = new Argon2Config
        {
            Type = Argon2Type.HybridAddressing, // Argon2id
            Version = Argon2Version.Nineteen,
            TimeCost = 3,                       // 3 iterations
            MemoryCost = 65536,                 // 64 MB
            Lanes = 4,                          // 4 parallel threads/lanes
            Threads = Math.Min(Environment.ProcessorCount, 4),
            Password = Encoding.UTF8.GetBytes(password),
            Salt = salt
        };

        // 3. Compute hash and encode in standard modular crypt / PHC format
        using var argon2 = new Argon2(config);
        using var hash = argon2.Hash();
        string hashString = config.EncodeString(hash.Buffer);

        return (hashString, saltBase64);
    }

    public bool VerifyPassword(string hashString, string candidatePassword)
    {
        // Argon2.Verify extracts algorithm, salt, memory, time cost, and parallelism directly from the PHC string
        return Argon2.Verify(hashString, candidatePassword);
    }
}
