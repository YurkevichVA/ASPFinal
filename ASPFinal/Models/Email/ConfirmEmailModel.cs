namespace ASPFinal.Models.Email
{
    public class ConfirmEmailModel
    {
        public string Name    { get; set; } = null!;
        public string Email       { get; set; } = null!;
        public string EmailCode   { get; set; } = null!;
        public string ConfirmLink { get; set; } = null!;
    }
}
