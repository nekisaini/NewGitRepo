using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategorie).ToListAsync(),
                Category = await _db.Category.ToListAsync(),
                Coupon = await _db.Coupon.Where(x => x.IsActive == true).ToListAsync()
        };
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if(claim!=null)
            {
                var count = _db.ShoppingCart.Where(x => x.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32("ssShoppingCartCount", count);
            }
            return View(indexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var MenuFromDB = await _db.MenuItem.Include(u => u.Category).Include(a => a.SubCategorie).Where(s=>s.Id==id).FirstOrDefaultAsync();
            ShoppingCart cart = new ShoppingCart()
            {
                MenuItem = MenuFromDB,
                MenuItemId = MenuFromDB.Id,
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart objCart)
        {
            objCart.Id = 0;
            if(ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                objCart.ApplicationUserId = claim.Value;

                ShoppingCart CartFromdb = await _db.ShoppingCart.Where(x => x.ApplicationUserId == objCart.ApplicationUserId && x.MenuItemId ==                             objCart.MenuItemId).FirstOrDefaultAsync();
                if(CartFromdb==null)
                {
                    await _db.ShoppingCart.AddAsync(objCart);
                }
                else
                {
                    CartFromdb.Count = CartFromdb.Count + objCart.Count;
                }

                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(x => x.ApplicationUserId == objCart.ApplicationUserId).ToList().Count();

                HttpContext.Session.SetInt32("ssShoppingCartCount", count);

                return RedirectToAction("Index");
            }
            else
            {

                var MenuFromDB = await _db.MenuItem.Include(u => u.Category).Include(a => a.SubCategorie).Where(s => s.Id == objCart.MenuItemId).FirstOrDefaultAsync();
                ShoppingCart cart = new ShoppingCart()
                {
                    MenuItem = MenuFromDB,
                    MenuItemId = MenuFromDB.Id,

                };
                return View(cart);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
