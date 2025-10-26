using System.ComponentModel.DataAnnotations;

namespace Movie_Reservation_System.DTOs.Account
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
    }
}
