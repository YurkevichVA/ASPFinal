using ASPFinal.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASPFinal.Controllers
{
    public class ProfileController : Controller
    {
        private readonly DataContext _dataContext;

        public ProfileController(DataContext dataContext)
        {
            _dataContext = dataContext;
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
    }
}
