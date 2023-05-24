namespace ASPFinal.Models.Profile
{
    public class EditProfileValidationModel
    {
        public String LoginMessage { get; set; } = null!;
        public String PasswordMessage { get; set; } = null!;
        public String RepeatPasswordMessage { get; set; } = null!;
        public String EmailMessage { get; set; } = null!;
        public String NameMessage { get; set; } = null!;
        public String AvatarMessage { get; set; } = null!;
    }
}
