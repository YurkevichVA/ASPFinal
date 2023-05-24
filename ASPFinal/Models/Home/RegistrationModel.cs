namespace ASPFinal.Models.Home
{
    public class RegistrationModel
    {
        public string Name { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string RepeatPassword { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsAgree { get; set; } = false;
        public IFormFile Avatar { get; set; } = null!;
    }
}
