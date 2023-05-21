using ASPFinal.Data.Entity;
using ASPFinal.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ASPFinal.Models.Admin;
using ASPFinal.Services.Random;

namespace ASPFinal.Controllers
{
    public class AdminController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;

        public AdminController(DataContext dataContext, IRandomService randomService)
        {
            _dataContext = dataContext;
            _randomService = randomService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AddItem(ItemModel itemModel)
        {
            bool isModelValid = true;
            ItemValidationModel validation = new();

            #region Name Validation
            if (String.IsNullOrEmpty(itemModel.Name))
            {
                validation.NameMessage = "Назва не може бути порожнею!";
                isModelValid = false;
            }
            if (_dataContext.Items.Any(u => u.Name == itemModel.Name))
            {
                validation.NameMessage = "Предмет з такою назвою вже існує!";
                isModelValid = false;
            }
            #endregion

            #region Content Validation
            string savedName = null!;
            if (itemModel.Content is null)
            {
                validation.ContentMessage = "Контент не може бути порожнім!";
                isModelValid = false;
            }
            else
            {
                // Генеруємо для файла нове ім'я, зберігаючи розширення
                string ext = Path.GetExtension(itemModel.Content.FileName);
                // TODO: перевірити розширення на перелік дозволених

                string path = "wwwroot/contents/";
                do
                {
                    savedName = _randomService.RandomFileName() + ext;
                    path = "wwwroot/contents/" + savedName;
                } while (System.IO.File.Exists(path));

                using FileStream fs = new(path, FileMode.Create);
                itemModel.Content.CopyTo(fs);
                ViewData["savedName"] = savedName;
            }
            #endregion

            #region Type Validation
            if (itemModel.Type < 1 || itemModel.Type > 2)
            {
                validation.TypeMessage = "Некоректний тип!";
                isModelValid = false;
            }
            #endregion

            #region Cost Validation
            if (itemModel.CostCoins <= 0)
            {
                validation.CostMessage = "Вартість повинна бути більше 0!";
                isModelValid = false;
            }
            #endregion

            if (isModelValid)
            {
                Item item = new()
                {
                    Id = Guid.NewGuid(),
                    Name = itemModel.Name,
                    Type = itemModel.Type - 1,
                    CostCoins = itemModel.CostCoins,
                    Content = savedName
                };
                
                _dataContext.Items.Add(item);
                _dataContext.SaveChanges();

                ViewData["isSuccess"] = "Предмет успішно додано!";

                return View("Index");
            }
            else // не всі форми влаідні - повертаємо на форму реєстрації
            {
                // передаємо дані щодо перевірок
                ViewData["validation"] = validation;
                // спосіб перейти на View з іншою назвою, ніж метод
                return View("Index");
            }
        }
    }
}
