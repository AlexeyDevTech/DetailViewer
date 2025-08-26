#pragma warning disable CS8618

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Api.Models
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