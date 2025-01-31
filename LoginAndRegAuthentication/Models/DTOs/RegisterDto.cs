using System.ComponentModel.DataAnnotations;

namespace AuthJwtApi.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }  // Accepts "User" or "Admin"
    }
}   