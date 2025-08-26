namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего хешированием и проверкой паролей.
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Хеширует указанный пароль.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <returns>Хеш пароля в формате Base64.</returns>
        string HashPassword(string password);

        /// <summary>
        /// Проверяет, соответствует ли пароль в открытом виде указанному хешу.
        /// </summary>
        /// <param name="password">Пароль в открытом виде.</param>
        /// <param name="hashedPassword">Хеш для сравнения.</param>
        /// <returns>True, если пароль верный, иначе false.</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}