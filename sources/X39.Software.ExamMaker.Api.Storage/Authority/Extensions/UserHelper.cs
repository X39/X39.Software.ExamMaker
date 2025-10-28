using System.Text;
using JetBrains.Annotations;
using Konscious.Security.Cryptography;

namespace X39.Software.ExamMaker.Api.Storage.Authority.Extensions;

/// <summary>
/// Provides helper methods for managing and verifying passwords for instances of the <see cref="User"/> class.
/// </summary>
[PublicAPI]
public static class UserHelper
{
    /// <summary>
    /// Sets the password for the specified user by generating a hashed password and salt using the Argon2i algorithm.
    /// </summary>
    /// <param name="self">The user instance for which the password is to be set.</param>
    /// <param name="password">The plaintext password to be hashed and set for the user.</param>
    /// <param name="secret">
    /// A secret byte array used as part of the hashing process to enhance security during password hashing.
    /// </param>
    public static void SetPassword(this Entities.User self, string password, byte[] secret)
    {
        HashPassword(password, secret, null, out var passwordHash, out var passwordSalt);
        self.PasswordHash = passwordHash;
        self.PasswordSalt = passwordSalt;
    }

    /// <summary>
    /// Verifies if the given plaintext password matches the hashed password for the specified user using the Argon2i algorithm.
    /// </summary>
    /// <param name="self">The user instance containing the stored password hash and salt.</param>
    /// <param name="password">The plaintext password to verify against the user's password hash.</param>
    /// <param name="secret">
    /// A secret byte array used as part of the hashing process to enhance security during password verification.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the provided password matches the stored password hash.
    /// </returns>
    public static bool VerifyPassword(this Entities.User self, string password, byte[] secret)
    {
        HashPassword(password, secret, self.PasswordSalt, out var passwordHash, out _);
        return passwordHash.SequenceEqual(self.PasswordHash);
    }

    /// <summary>
    /// Generates a hashed password and corresponding salt using the Argon2i algorithm.
    /// </summary>
    /// <param name="password">The input password to be hashed.</param>
    /// <param name="secret">A secret byte array used as part of the hashing process to add another level of security.</param>
    /// <param name="salt">
    /// An optional byte array representing the salt. If null, a new random salt will be generated.
    /// The salt ensures that the resulting hash is unique even for identical passwords.
    /// </param>
    /// <param name="passwordHash">
    /// An output parameter that contains the generated password hash as a byte array upon method completion.
    /// </param>
    /// <param name="passwordSalt">
    /// An output parameter that contains the salt used in the hashing process as a byte array upon method completion.
    /// </param>
    private static void HashPassword(
        string password,
        byte[] secret,
        byte[]? salt,
        out byte[] passwordHash,
        out byte[] passwordSalt
    )
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        if (salt is null)
        {
            passwordSalt = new byte[256];
            using var randomByteGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
            randomByteGenerator.GetBytes(passwordSalt);
        }
        else
        {
            passwordSalt = salt;
        }

        using var algorithm = new Argon2i(passwordBytes);
        algorithm.Salt        = passwordSalt;
        algorithm.KnownSecret = secret;
        algorithm.MemorySize  = 65536; // 64 MB
        algorithm.Iterations  = 4;
        algorithm.DegreeOfParallelism  = 2;
        passwordHash          = algorithm.GetBytes(256);
    }
}
