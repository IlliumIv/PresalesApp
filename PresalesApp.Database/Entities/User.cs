using Microsoft.AspNetCore.Identity;

namespace PresalesApp.Database.Entities
{
    public class User : IdentityUser
    {
        [ProtectedPersonalData]
        public virtual string? ProfileName { get; set; }
    }
}
