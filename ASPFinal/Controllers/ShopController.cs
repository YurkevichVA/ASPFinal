using ASPFinal.Data;
using ASPFinal.Data.Entity;
using ASPFinal.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASPFinal.Controllers
{
    public class ShopController : Controller
    {
        private readonly DataContext _dataContext;

        public ShopController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            User? user = null;

            if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
            {
                user = _dataContext.Users.Find(
                        Guid.Parse(
                            HttpContext.User.Claims
                            .First(c => c.Type == ClaimTypes.Sid)
                            .Value
                    ));
            }

            ShopModel model = new ShopModel()
            {
                Items = _dataContext
                    .Items
                    .Where(i => i.DeleteDt == null)
                    .AsEnumerable()
                    .Select(i => new ItemModel()
                    {
                        Name = i.Name,
                        Content = i.Content,
                        CostCoins = i.CostCoins,
                        Type = (i.Type == 1 ? "Персонаж" : "Предмет"),
                        Id = i.Id.ToString(),
                        IsActive = HttpContext.User.Identity?.IsAuthenticated == true
                            ? (user.Transactions is null 
                                ? true 
                                : (_dataContext.Transactions.Where(t => t.ItemId == i.Id && t.UserId == user.Id).Count() > 0 
                                    ? false : true)) : false
                    })
                    .ToList()
            };
            
            return View(model);
        }
        [HttpPost]
        public string BuyItem([FromBody] BuyItemModel buyItemModel)
        {
            if (HttpContext.User.Identity is null || !HttpContext.User.Identity.IsAuthenticated)
            {
                return "Authentication required";
            }

            User? user = _dataContext.Users.Find(
                        Guid.Parse(
                            HttpContext.User.Claims
                            .First(c => c.Type == ClaimTypes.Sid)
                            .Value
                    ));

            if (user == null)
            {
                return "Authentication required";
            }

            Item? item = _dataContext.Items.Find(Guid.Parse(buyItemModel.Id));

            if(item == null)
            {
                return "Item not found";
            }

            if(_dataContext.Transactions.Where(t => t.ItemId == item.Id && t.UserId == user.Id).Count() > 0)
            {
                return "You alredy have this item";
            }

            if(user.CoinsCount < item.CostCoins)
            {
                return "Not enough coins";
            }

            user.CoinsCount = user.CoinsCount - item.CostCoins;

            _dataContext.Transactions.Add(new()
            {
                ItemId = item.Id,
                UserId = user.Id,
                TransactionDate = DateTime.Now,
            });

            _dataContext.SaveChanges();

            return "OK";
        }
    }
}
