using System.Net;
using System.Net.Mail;
using System.Runtime.Intrinsics.X86;

namespace ASPFinal.Services.Email
{
    public class GmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailService> _logger;

        public GmailService(IConfiguration configuration, ILogger<GmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool Send(string mailTemplate, object model)
        {
            // Шукаємо шаблон листа
            string? template = null;
            string[] filenames = new string[] {
                mailTemplate,
                mailTemplate + ".html",
                "Services/Email/" + mailTemplate,
                "Services/Email/" + mailTemplate + ".html"
            };

            foreach (string filename in filenames)
            {
                if (File.Exists(filename))
                {
                    template = File.ReadAllText(filename);
                    break;
                }
            }

            if(template is null)
            {
                throw new ArgumentException($"Template '{mailTemplate}' not found");
            }

            // Перевіряємо поштову конфігурацію
            string? host     = _configuration["Smtp:Gmail:Host"];
            if(host is null) { throw new MissingFieldException("Missing configuration 'Smtp:Gmail:Host'"); }
            string? mailbox  = _configuration["Smtp:Gmail:Email"];
            if (host is null) { throw new MissingFieldException("Missing configuration 'Smtp:Gmail:Email'"); }
            string? password = _configuration["Smtp:Gmail:Password"];
            if (host is null) { throw new MissingFieldException("Missing configuration 'Smtp:Gmail:Password'"); }

            int port;
            try { port = Convert.ToInt32(_configuration["Smtp:Gmail:Port"]); }
            catch { throw new MissingFieldException("Missing configuration 'Smtp:Gmail:Port'"); }
            bool ssl;
            try { ssl = Convert.ToBoolean(_configuration["Smtp:Gmail:Ssl"]); }
            catch { throw new MissingFieldException("Missing configuration 'Smtp:Gmail:Host'"); }

            // Заповнюємо шаблон - проходимо по властивостях моделі та замінюємо їх значення у шаблоні за збігом імен
            string? userEmail = null;
            foreach(var prop in model.GetType().GetProperties())
            {
                if(prop.Name == "Email") userEmail = prop.GetValue(model)?.ToString();
                string placeholder = $"{{{{{prop.Name}}}}}";
                if(template.Contains(placeholder))
                {
                    template = template.Replace(placeholder, prop.GetValue(model)?.ToString() ?? "");
                }

            }
            if(userEmail is null) 
            {
                throw new ArgumentException("No 'Email' property in model");
            }
            // TODO: перевірити залишок {{\w+}} плейсхолдерів у шаблоні
            using SmtpClient smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
            MailMessage mailMessage = new()
            {
                From = new MailAddress(mailbox),
                Subject = "ASP-201 Project",
                IsBodyHtml = true,
                Body = template
            };
            mailMessage.To.Add(userEmail);
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Send Email exception '{ex}'", ex.Message);
                return true;
            }
        }
    }
}
