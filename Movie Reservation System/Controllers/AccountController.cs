using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Movie_Reservation_System.DTOs.Account;
using MovieReservationSystem.Core.Entities;
using MovieReservationSystem.Core.Service.Contract;

namespace Movie_Reservation_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMailService _mailService;
        private readonly ILogger<AccountController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService;

        public AccountController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IMailService mailService,
            ILogger<AccountController> logger,
            RoleManager<IdentityRole> roleManager,
            IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _logger = logger;
            _roleManager = roleManager;
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                Name = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { status = "Error", message = "User creation failed", errors = result.Errors });

            // Add user to role
            if (await _roleManager.RoleExistsAsync("User"))
                await _userManager.AddToRoleAsync(user, "User");
            else
                return BadRequest("User role does not exist");

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Build confirmation link
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email }, Request.Scheme);

            // Send confirmation email
            var email = new Email
            {
                To = user.Email,
                Subject = "Confirm your email",
                Body = $"Please confirm your email by clicking this link: \n{confirmationLink}"
            };

            _mailService.SendEmail(email);

            return Ok(new { status = "Success", message = "User created & confirmation email sent successfully." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { message = "Please confirm your email before logging in." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid email or password." });

            // Generate JWT token
            var token = await _authService.CreateTokenAsync(user, _userManager);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                status = "Success",
                message = "Login successful!",
                token = token,
                user = new
                {
                    user.UserName,
                    user.Email,
                    user.Name,
                    role = roles.FirstOrDefault()
                }
            });
        }

        [HttpGet("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { status = "Error", message = "User not found." });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return Ok(new { status = "Success", message = "Email verified successfully." });

            return StatusCode(StatusCodes.Status500InternalServerError,
                new { status = "Error", message = "Email verification failed." });
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("ForgotPassword: No user found for email {Email}", email);
                return BadRequest(new { Status = "Error", Message = "Could not send reset code. Please try again." });
            }

            try
            {
                // Generate 6-digit OTP
                var otpCode = new Random().Next(100000, 999999).ToString();

                // Store OTP & expiration in database
                user.PasswordResetCode = otpCode;
                user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                // Send OTP email
                var emailMessage = new Email
                {
                    To = user.Email,
                    Subject = "Password Reset Code",
                    Body = $"Your password reset code is: {otpCode}\n\nThis code will expire in 10 minutes."
                };

                _mailService.SendEmail(emailMessage);

                return Ok(new { Status = "Success", Message = $"Password reset code has been sent to {user.Email}. Please check your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError("ForgotPassword: Error sending email for {Email}. Exception: {Exception}", email, ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Status = "Error", Message = "Failed to send reset code. Please try again later." });
            }
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("ResetPassword: No user found for email {Email}", model.Email);
                return BadRequest(new { Status = "Error", Message = "Invalid email or reset code." });
            }

            if (user.PasswordResetCode != model.Code || user.ResetCodeExpiration < DateTime.UtcNow)
            {
                _logger.LogWarning("ResetPassword: Invalid or expired reset code for {Email}", model.Email);
                return BadRequest(new { Status = "Error", Message = "Invalid or expired reset code." });
            }

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                _logger.LogError("ResetPassword: Failed to remove old password for {Email}", model.Email);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Status = "Error", Message = "Password reset failed. Please try again." });
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addResult.Succeeded)
            {
                user.PasswordResetCode = null;
                user.ResetCodeExpiration = null;
                await _userManager.UpdateAsync(user);

                return Ok(new { Status = "Success", Message = "Password has been successfully reset." });
            }

            _logger.LogError("ResetPassword: Password reset failed for user {Email}", model.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { Status = "Error", Message = "Password reset failed. Please try again." });
        }
    }
}