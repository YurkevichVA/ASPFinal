using Microsoft.AspNetCore.Mvc;

namespace ASPFinal.Models.Profile
{
    public class EditProfileModel
    {
        [FromForm(Name = "form-name")]
        public string Name { get; set; } = null!;
        [FromForm(Name = "form-login")]
        public string Login { get; set; } = null!;
        [FromForm(Name = "form-email")]
        public string Email { get; set; } = null!;
        [FromForm(Name = "form-avatar")]
        public IFormFile? Avatar { get; set; }
    }
}
