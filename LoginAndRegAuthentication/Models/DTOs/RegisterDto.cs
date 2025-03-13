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

        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public string state { get; set; }


        public string Address { get; set; }

        [Required]
        public string Role { get; set; }  // Accepts "User" or "Admin"
    }
}   