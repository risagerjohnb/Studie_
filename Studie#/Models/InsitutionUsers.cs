using Microsoft.AspNetCore.Identity;

namespace Studie_.Models
{
    public class InstitutionUsers : IdentityUser
    {
        public string InstitutionId { get; set; } = string.Empty;
        public Institutions? Institution { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public bool EmailNotificationsEnabled { get; set; } = false;
    }
}
