namespace ASPFinal.Services.Kdf
{
    /// <summary>
    /// Key Derivation Function (RFC 8018)
    /// </summary>
    public interface IKdfService
    {
        /// <summary>
        /// Mixing password and salt to make a derived key
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="salt">Salt</param>
        /// <returns>Derived Key as string</returns>
        string GetDerivedKey(string password, string salt);
    }
}
