using DetailViewer.Core.Interfaces;
using System.Security.Cryptography;
using System;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для хеширования и проверки паролей с использованием PBKDF2.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        private readonly ILogger _logger;
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="PasswordService"/>.
        /// </summary>
        /// <param name="logger">Сервис логирования.</param>
        public PasswordService(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public string HashPassword(string password)
        {
            _logger.Log("Hashing password");
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <inheritdoc/>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            _logger.Log("Verifying password");
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            for (int i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}