namespace ASPFinal.Services.Email
{
    public interface IEmailService
    {
        bool Send(string mailTemplate, object model);
    }
}
