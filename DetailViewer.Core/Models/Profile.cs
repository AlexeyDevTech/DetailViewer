using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    public enum Role
    {
        Admin,
        Moderator,
        Operator
    }

    public class Profile
    {
        [Key]
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public Role Role { get; set; }
        public string PasswordHash { get; set; } // В реальном приложении здесь должен быть хэш

        [NotMapped]
        public string FullName => $"{LastName} {FirstName} {MiddleName}".Trim();
    }
}