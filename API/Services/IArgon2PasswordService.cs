namespace API.Services;

public interface IArgon2PasswordService
{
    /// <summary>
    /// Hashes the raw password using Argon2id with a cryptographically secure random salt (OWASP standard).
    /// </summary>
    (string HashString, string SaltBase64) HashPassword(string password);

    /// <summary>
    /// Verifies whether the provided candidate password matches the Argon2id hash.
    /// Hashes are one-way and cannot be decrypted; verification is done via constant-time comparison.
    /// </summary>
    bool VerifyPassword(string hashString, string candidatePassword);
}
