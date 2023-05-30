using ASPFinal.Data;
using ASPFinal.Models.Home;
using ASPFinal.Models.Profile;
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

        public ProfileController(DataContext dataContext, IValidationService validationService, IRandomService randomService, ILogger<ProfileController> logger)
        {
            _dataContext = dataContext;
            _validationService = validationService;
            _randomService = randomService;
            _logger = logger;
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
    }
}
