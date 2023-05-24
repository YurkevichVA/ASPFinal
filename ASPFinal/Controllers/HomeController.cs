using ASPFinal.Data.Entity;
using ASPFinal.Data;
using ASPFinal.Models;
using ASPFinal.Models.Home;
using ASPFinal.Services.Random;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ASPFinal.Services.Validation;
using ASPFinal.Services.Kdf;
using ASPFinal.Services.Email;
using Microsoft.Extensions.Primitives;

namespace ASPFinal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IValidationService _validationService;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;
        private readonly IEmailService _emailService;
        private readonly DataContext _dataContext;

        public HomeController(ILogger<HomeController> logger, DataContext dataContext, IValidationService validationService, IRandomService randomService, IKdfService kdfService, IEmailService emailService)
        {
            _logger = logger;
            _dataContext = dataContext;
            _validationService = validationService;
            _randomService = randomService;
            _kdfService = kdfService;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration(RegistrationModel registrationModel)
        {
            bool isModelValid = true;
            byte minPasswordLength = 3;
            RegistrationValidationModel registerValidation = new();

            #region Login Validation
            if (String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін не може бути порожнім!";
                isModelValid = false;
            }
            if (_dataContext.Users.Any(u => u.Login == registrationModel.Login))
            {
                registerValidation.LoginMessage = "Логін вже використовується!";
                isModelValid = false;
            }
            #endregion

            #region Password / Repeat Validation
            if (String.IsNullOrEmpty(registrationModel.Password))
            {
                registerValidation.PasswordMessage = "Треба ввести пароль!";
                registerValidation.RepeatPasswordMessage = "";
                isModelValid = false;
            }
            else if (registrationModel.Password.Length < minPasswordLength)
            {
                registerValidation.PasswordMessage = $"Пароль закороткий. Щонайменьше {minPasswordLength} символів";
                registerValidation.RepeatPasswordMessage = "";
                isModelValid = false;
            }
            else if (!registrationModel.Password.Equals(registrationModel.RepeatPassword))
            {
                registerValidation.PasswordMessage =
                    registerValidation.RepeatPasswordMessage = "Паролі не збігаються!";
                isModelValid = false;
            }

            if (String.IsNullOrEmpty(registrationModel.RepeatPassword))
            {
                registerValidation.RepeatPasswordMessage = "Треба повторити пароль!";
                isModelValid = false;
            }

            #endregion

            #region Email Validation
            if (!_validationService.Validate(registrationModel.Email, ValidationTerms.NotEmpty))
            {
                registerValidation.EmailMessage = "Необхідно ввести e-mail!";
                isModelValid = false;
            }
            else if (!_validationService.Validate(registrationModel.Email, ValidationTerms.Email))
            {
                registerValidation.EmailMessage = "Email не відповідає шаблону";
                isModelValid = false;
            }
            #endregion

            #region Name Validation
            if (String.IsNullOrEmpty(registrationModel.Name))
            {
                registerValidation.NameMessage = "Ім'я не може бути порожнім!";
                isModelValid = false;
            }
            else
            {
                String nameRegex = @"^.+$";
                if (!Regex.IsMatch(registrationModel.Name, nameRegex))
                {
                    registerValidation.NameMessage = "Ім'я не відповідає шаблону";
                    isModelValid = false;
                }
            }
            #endregion

            #region Avatar
            // вважаємо аватар не обов'язковим
            string savedName = null!;
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length < 1024)
                {
                    registerValidation.AvatarMessage = "Файл замалий!";
                    isModelValid = false;
                }
                else
                {
                    // Генеруємо для файла нове ім'я, зберігаючи розширення
                    string ext = Path.GetExtension(registrationModel.Avatar.FileName);
                    // TODO: перевірити розширення на перелік дозволених

                    string path = "wwwroot/avatars/";
                    do
                    {
                        savedName = _randomService.RandomFileName() + ext;
                        path = "wwwroot/avatars/" + savedName;
                    } while (System.IO.File.Exists(path));

                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                    ViewData["savedName"] = savedName;
                }
            }
            else
            {
                savedName = "no-avatar.png";
            }
            #endregion

            #region IsAgree Validation
            if (!registrationModel.IsAgree)
            {
                registerValidation.IsAgreeMessage = "Необхідно погодитись з правилами сайту!";
                isModelValid = false;
            }
            #endregion

            // якщо всі перевірки пройдені, то переходимо на нову сторінку з вітаннями
            if (isModelValid)
            {
                string salt = _randomService.RandomString(16);
                string confirmEmailCode = _randomService.ConfirmCode(6);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    Name = registrationModel.Name,
                    Email = registrationModel.Email,
                    EmailCode = confirmEmailCode,
                    PasswordSalt = salt,
                    PasswordHash = _kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegistrationDt = DateTime.Now
                };
                _dataContext.Users.Add(user);
                // Якщо дані до БД додано, надсилаємо код підтвердження на пошту
                // Генеруємо токен автоматичного підтвердження
                var emailConfirmToken = _GenerateEmailConfirmToken(user);

                // Надсилаємо листа з токеном
                _SendConfirmEmail(user, emailConfirmToken);

                _dataContext.SaveChanges();

                _emailService.Send(
                    "welcome_letter",
                    new Models.Email.ConfirmEmailModel
                    {
                        Email = user.Email,
                        Name = user.Name,
                        EmailCode = user.EmailCode,
                        ConfirmLink = "#"
                    });

            }
            else // не всі форми влаідні - повертаємо на форму реєстрації
            {
                // передаємо дані щодо перевірок
                ViewData["registerValidation"] = registerValidation;
            }
            return View();
        }
        [HttpPost]
        public IActionResult AuthUser(LoginModel loginModel)
        {
            if (loginModel.Login is null)
            {
                ViewData["auth-msg"] = "Missed required parameter: user-login";
                return RedirectToAction("Registration");
            }
            if (loginModel.Password is null)
            {
                ViewData["auth-msg"] = "Missed required parameter: user-password";
                return RedirectToAction("Registration");
            }

            string login = loginModel.Login;
            string password = loginModel.Password;

            User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
            if (user is not null)
            {
                // якщо знайшли - перевіряємо пароль (derived key)
                if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
                {
                    // дані перевірені - користувач автентифікований - зберігаємо у сесії
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return RedirectToAction("Index", "Profile", new { id = user.Login });
                }
            }

            ViewData["auth-msg"] = "Авторизацію відхилено";
            return RedirectToAction("Registration");
        }
        [HttpGet]
        public IActionResult RedirectToProfile()
        {
            _logger.Log(LogLevel.Debug, "red to prof");
            return RedirectToAction("Index", "Profile");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Private methods
        private EmailConfirmToken _GenerateEmailConfirmToken(User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                Moment = DateTime.Now,
                Used = 0
            };
            _dataContext.EmailConfirmTokens.Add(emailConfirmToken);
            return emailConfirmToken;
        }
        private bool _SendConfirmEmail(User user, EmailConfirmToken token)
        {
            // Формуємо посилання: схема://домен/User/ConfirmToken?token=...
            string confirmLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/User/ConfirmToken?token={token.Id}";

            return _emailService.Send(
                "confirm_email",
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    Name = user.Name,
                    EmailCode = user.EmailCode!,
                    ConfirmLink = confirmLink
                });
        }
    }
}