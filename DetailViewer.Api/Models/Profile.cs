#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Определяет роли пользователей в системе.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Администратор системы.
        /// </summary>
        Admin,
        /// <summary>
        /// Модератор, имеющий расширенные права.
        /// </summary>
        Moderator,
        /// <summary>
        /// Обычный пользователь системы.
        /// </summary>
        Operator
    }

    public class Profile
    {
        /// <summary>
        /// Получает или устанавливает уникальный идентификатор профиля.
        /// </summary>
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Получает или устанавливает фамилию пользователя.
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Получает или устанавливает имя пользователя.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Получает или устанавливает отчество пользователя.
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Получает или устанавливает роль пользователя.
        /// </summary>
        public Role Role { get; set; }
        /// <summary>
        /// Получает или устанавливает хэш пароля пользователя.
        /// </summary>
        public string PasswordHash { get; set; } // В реальном приложении здесь должен быть хэш

        /// <summary>
        /// Получает полное имя пользователя (Фамилия Имя Отчество).
        /// </summary>
        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        /// <summary>
        /// Получает сокращенное имя пользователя (Фамилия И.О.).
        /// </summary>
        [NotMapped]
        public string ShortName
        {
            get
            {
                var firstInitial = !string.IsNullOrEmpty(FirstName) ? $"{FirstName[0]}." : string.Empty;
                var middleInitial = !string.IsNullOrEmpty(MiddleName) ? $"{MiddleName[0]}." : string.Empty;
                return $"{LastName} {firstInitial}{middleInitial}".Trim();
            }
        }
    }
}