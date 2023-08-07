namespace UserService.Authentication.Models
{
    public class TokenSettings
    {
        public TokenSettings(string encryptionKey, string issuer)
        {
            EncryptionKey = encryptionKey;
            Issuer = issuer;
            Expiration = DateTime.UtcNow.AddHours(6);
        }

        /// <summary>
        /// Gets the encryption key
        /// </summary>
        public string EncryptionKey { get; }

        /// <summary>
        /// Gets the date when the session will expire
        /// </summary>
        public DateTime Expiration { get; }

        /// <summary>
        /// Gets or sets the issuer
        /// </summary>
        public string Issuer { get; private set; }
    }
}
