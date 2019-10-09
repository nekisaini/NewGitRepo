using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class MnuItemController : Controller
    {

        private readonly ApplicationDbContext _db;

        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel menuItemVM { get; set; }

        public MnuItemController(ApplicationDbContext db,IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            menuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuitem = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategorie).ToListAsync();
            return View(menuitem);
        }
        public ActionResult Create()
        {
            return View(menuItemVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            menuItemVM.MenuItem.SubCategoryId= Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if(!ModelState.IsValid)
            {
                return View(menuItemVM);
            }
            _db.MenuItem.Add(menuItemVM.MenuItem);

            await _db.SaveChangesAsync();
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(menuItemVM.MenuItem.Id);

            if(files.Count>0)
            {
                var uploads = Path.Combine(webRootPath , "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, menuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + menuItemVM.MenuItem.Id + extension;
            }
            else
            {
                var uploads = Path.Combine(webRootPath , @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads,webRootPath + @"\images\"+menuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + menuItemVM.MenuItem.Id + ".png";

            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            menuItemVM.MenuItem = await _db.MenuItem.Include(d => d.Category).Include(d => d.SubCategorie).SingleOrDefaultAsync(m => m.Id == id);

            menuItemVM.SubCategory = await _db.SubCategorie.Where(m => m.CategoryId == menuItemVM.MenuItem.CategoryId).ToListAsync();
            if(menuItemVM.MenuItem==null)
            {
                return NotFound();
            }
            return View(menuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            menuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                menuItemVM.SubCategory = await _db.SubCategorie.Where(s => s.CategoryId == menuItemVM.MenuItem.CategoryId).ToListAsync();

                return View(menuItemVM);
            }
         
            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItem.FindAsync(menuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                    
                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                using (var fileStream = new FileStream(Path.Combine(uploads, menuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                menuItemFromDb.Image = @"\images\" + menuItemVM.MenuItem.Id + extension_new;
            }
            menuItemFromDb.Name = menuItemVM.MenuItem.Name;
            menuItemFromDb.Description= menuItemVM.MenuItem.Description;
            menuItemFromDb.Price = menuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = menuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = menuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = menuItemVM.MenuItem.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

      
        public async Task<IActionResult> Delete(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            menuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategorie).FirstOrDefaultAsync(m => m.Id == id);


            if(menuItemVM.MenuItem==null)
            {
                return NotFound();
            }
            return View(menuItemVM);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;

            MenuItem menuItem = await _db.MenuItem.FindAsync(id);

            if(menuItem!=null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItem.Remove(menuItem);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            menuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategorie).FirstOrDefaultAsync(m => m.Id == id);
            if(menuItemVM.MenuItem==null)
            {
                return NotFound();
            }

            return View(menuItemVM);
        }
    }
}