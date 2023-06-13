using ASPFinal.Data;
using ASPFinal.Data.Entity;
using ASPFinal.Models;
using ASPFinal.Models.Admin;
using ASPFinal.Models.Home;
using ASPFinal.Models.Profile;
using ASPFinal.Services.Email;
using ASPFinal.Services.Random;
using ASPFinal.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ASPFinal.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IValidationService _validationService;
        private readonly IRandomService _randomService;
        private readonly ILogger<ProfileController> _logger;
        private readonly IEmailService _emailService;

        public ProfileController(DataContext dataContext, IValidationService validationService, IRandomService randomService, ILogger<ProfileController> logger, IEmailService emailService)
        {
            _dataContext = dataContext;
            _validationService = validationService;
            _randomService = randomService;
            _logger = logger;
            _emailService = emailService;
        }

        public IActionResult Index([FromRoute] string id)
        {
            return RedirectToAction("Profile", new { id });
        }
        public IActionResult Profile([FromRoute] string id) 
        {
            Data.Entity.User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                Models.Profile.ProfileModel model = new(user);
                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    string userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                    if (userLogin == user.Login)
                    {
                        model.IsPersonal = true;
                    }
                }

                model.Items = new();

                var transactions = _dataContext.Transactions.Where(t => t.UserId == user.Id).ToList();

                if (_dataContext.Transactions.Where(t => t.UserId == user.Id).Count() > 0)
                {
                    foreach (var transaction in transactions)
                    {
                        model.Items.Add(new ProfileItemModel()
                        {
                            Content = _dataContext.Items.Where(i => i.Id == transaction.ItemId).FirstOrDefault().Content
                        });
                    }
                }

                model.CharactersCount = _dataContext.Transactions.Where(t => t.UserId == user.Id).Count();
                
                return View(model);
            }
            else
            {
                return NotFound();
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult EditProfile([FromForm] EditProfileModel editProfileModel)
        {
            Guid userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
            EditProfileValidationModel validation = new EditProfileValidationModel();
            bool isModelValid = true;
            Data.Entity.User user = _dataContext.Users.FirstOrDefault(u => u.Id == userId);

            #region Login Validation
            if (user.Login != editProfileModel.Login)
            {
                if (String.IsNullOrEmpty(editProfileModel.Login) || !_validationService.Validate(editProfileModel.Login, ValidationTerms.Login))
                {
                    validation.LoginMessage = "Логін не може бути порожнім!";
                    isModelValid = false;
                }
                else if (_dataContext.Users.Any(u => u.Login == editProfileModel.Login))
                {
                    validation.LoginMessage = "Логін вже використовується!";
                    isModelValid = false;
                }
                else
                {
                    user.Login = editProfileModel.Login;
                }
            }
            #endregion

            #region Email Validation
            if (user.Email != editProfileModel.Email)
            {
                if (!_validationService.Validate(editProfileModel.Email, ValidationTerms.NotEmpty))
                {
                    validation.EmailMessage = "Необхідно ввести e-mail!";
                    isModelValid = false;
                }
                else if (!_validationService.Validate(editProfileModel.Email, ValidationTerms.Email))
                {
                    validation.EmailMessage = "Email не відповідає шаблону";
                    isModelValid = false;
                }
                else
                {
                    user.Email = editProfileModel.Email;
                }
            }
            #endregion

            #region Name Validation
            if (user.Name != editProfileModel.Name)
            {
                if (!_validationService.Validate(editProfileModel.Name, ValidationTerms.NotEmpty))
                {
                    validation.NameMessage = "Ім'я не може бути порожнім!";
                    isModelValid = false;
                }
                else if (!_validationService.Validate(editProfileModel.Name, ValidationTerms.RealName))
                {
                    validation.NameMessage = "Ім'я не відповідає шаблону";
                    isModelValid = false;
                }
                else
                {
                    user.Name = editProfileModel.Name;
                }
            }
            #endregion

            #region Avatar
            string savedName = null!;
            if (editProfileModel.Avatar is not null)
            {
                if (editProfileModel.Avatar.Length < 1024)
                {
                    validation.AvatarMessage = "Файл замалий!";
                    isModelValid = false;
                }
                else
                {
                    // Генеруємо для файла нове ім'я, зберігаючи розширення
                    string ext = Path.GetExtension(editProfileModel.Avatar.FileName);
                    // TODO: перевірити розширення на перелік дозволених

                    string path = "wwwroot/avatars/";
                    do
                    {
                        savedName = _randomService.RandomFileName() + ext;
                        path = "wwwroot/avatars/" + savedName;
                    } while (System.IO.File.Exists(path));

                    using FileStream fs = new(path, FileMode.Create);
                    editProfileModel.Avatar.CopyTo(fs);
                    user.Avatar = savedName;
                }
            }
            #endregion


            _dataContext.SaveChanges();
            ViewData["validation"] = validation;

            return RedirectToAction("Profile", new { id = user.Login });
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] string emailCode)
        {
            StatusDataModel model = new();
            if (string.IsNullOrEmpty(emailCode))
            {
                model.Status = "406";
                model.Data = "Empty code not acceptable";
            }
            else if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                model.Status = "401";
                model.Data = "Unauthenticated";
            }
            else
            {
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value
                ));
                if (user is null)
                {
                    model.Status = "403";
                    model.Data = "Forbidden (UnAuthorized)";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if (user.EmailCode != emailCode)
                {
                    model.Status = "406";
                    model.Data = "Code not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model);
        }
        [HttpGet]
        public ViewResult ConfirmToken([FromQuery] string token)
        {
            try
            {
                var confirmToken = _dataContext.EmailConfirmTokens.Find(Guid.Parse(token)) ?? throw new Exception();
                var user = _dataContext.Users.Find(confirmToken.UserId) ?? throw new Exception();
                // перевіряємо збіг поштових адрес
                if (user.Email != confirmToken.UserEmail) throw new Exception();
                // Оновлюємо дані
                user.EmailCode = null;  // пошта підтверджена
                confirmToken.Used += 1; // ведемо підрахунок використання токена
                _dataContext.SaveChanges();
                ViewData["result"] = "Вітаємо, пошта успішно підтверджена";
            }
            catch
            {
                ViewData["result"] = "Перевірка не пройдена, не змінюйте посилання з листа!";
            }
            return View();
        }
        [HttpPatch]
        public string ResendConfirmEmail()
        {
            if (HttpContext.User.Identity?.IsAuthenticated == false)
            {
                return "Unauthenticated";
            }
            try
            {
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value)) ?? throw new Exception();

                user.EmailCode = _randomService.ConfirmCode(6);

                var emailConfirmToken = _GenerateEmailConfirmToken(user);

                _dataContext.SaveChangesAsync();

                if (_SendConfirmEmail(user, emailConfirmToken)) return "OK";
                else return "Send error";
            }
            catch
            {
                return "Unauthorized";
            }
        }
        [HttpGet] 
        public string AddCoins() 
        {
            try
            {
                User? user = _dataContext.Users.Find(
                        Guid.Parse(
                            HttpContext.User.Claims
                            .First(c => c.Type == ClaimTypes.Sid)
                            .Value
                    ));
                user.CoinsCount = user.CoinsCount + _randomService.RandomCoins();
                _dataContext.SaveChanges();
                return "OK";
            }
            catch
            {
                return "ERROR";
            }
        }

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
            string confirmLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/Profile/ConfirmToken?token={token.Id}";

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
