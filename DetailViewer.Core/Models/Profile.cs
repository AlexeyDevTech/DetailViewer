using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
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
        /// Модератор данных.
        /// </summary>
        Moderator,

        /// <summary>
        /// Оператор, работающий с данными.
        /// </summary>
        Operator
    }

    /// <summary>
    /// Представляет профиль пользователя системы.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// Уникальный идентификатор профиля.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Отчество пользователя.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Роль пользователя в системе.
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// Хеш пароля пользователя.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Полное имя пользователя (Фамилия Имя Отчество).
        /// </summary>
        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();

        /// <summary>
        /// Краткое имя пользователя (Фамилия И.О.).
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
