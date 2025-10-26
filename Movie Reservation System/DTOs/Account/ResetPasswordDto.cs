namespace Movie_Reservation_System.DTOs.Account
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string Code { get; set; } // OTP Code
        public string NewPassword { get; set; }
    }
}
