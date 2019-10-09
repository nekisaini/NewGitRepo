using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;


        [TempData]
        public string StatusMessage { get; set; }


        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var category = await _db.SubCategorie.Include(s => s.Category).ToListAsync();
            return View(category);
        }

        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel VM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategorie.OrderBy(p => p.Name).Select(s => s.Name).Distinct().ToListAsync()
            };
            return View(VM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExist = _db.SubCategorie.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExist.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category exists under " + doesSubCategoryExist.First().Category.Name + " category. Please use another name.";
                }
                else
                {
                    _db.SubCategorie.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategorie.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);

        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from subCategory in _db.SubCategorie
                           where subCategory.CategoryId == id
                           select subCategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategorie.SingleOrDefaultAsync(s => s.Id == id);

            if(subCategory==null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel VM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategorie.OrderBy(p => p.Name).Select(s => s.Name).Distinct().ToListAsync()
            };
            return View(VM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExist = _db.SubCategorie.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExist.Count() > 0)
                {
                    StatusMessage = "Error: Sub Category exists under " + doesSubCategoryExist.First().Category.Name + " category. Please use another name.";
                }
                else
                {
                    var subCatFromDb = await _db.SubCategorie.FindAsync(model.SubCategory.Id);
                    subCatFromDb.Name = model.SubCategory.Name;
                   
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategorie.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            return View(modelVM);

        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategorie.Include(m => m.Category).SingleOrDefaultAsync(s => s.Id == id);
            if(subCategory==null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategorie.Include(m => m.Category).SingleOrDefaultAsync(s => s.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }
            return View(subCategory);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var subCategory = await _db.SubCategorie.SingleOrDefaultAsync(s => s.Id == id);
            _db.SubCategorie.Remove(subCategory);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}