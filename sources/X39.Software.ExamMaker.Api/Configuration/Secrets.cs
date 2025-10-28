namespace X39.Software.ExamMaker.Api.Configuration;

public sealed class Secrets
{
    /// <summary>
    /// Gets or sets the salt value encoded as a Base64 string.
    /// The salt is used for cryptographic operations and should be stored securely.
    /// </summary>
    public string SaltBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Converts the SaltBase64 string property to a byte array.
    /// The SaltBase64 property is expected to be a Base64-encoded string representation of the salt value.
    /// This method is used to retrieve the salt value as a byte array for cryptographic operations.
    /// </summary>
    /// <returns>A byte array representing the decrypted salt derived from the SaltBase64 string.</returns>
    public byte[] GetSalt() => Convert.FromBase64String(SaltBase64);
}
