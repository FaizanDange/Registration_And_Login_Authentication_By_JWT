using System.Collections.Generic;

namespace AuthJwtApi.Models.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation property
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}