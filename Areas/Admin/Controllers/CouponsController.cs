using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Spice.Utility;
using Microsoft.AspNetCore.Authorization;
using Spice.Data;
using Spice.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.ManagerUser)]
    public class CouponsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            fs1.CopyTo(ms);
                            p1 = ms.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }
                _db.Coupon.Add(coupon);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);

        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (coupon.Id == 0)
            {
                return NotFound();
            }

            var couponFromDB = await _db.Coupon.Where(m => m.Id == coupon.Id).FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;

                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var rs = files[0].OpenReadStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            rs.CopyTo(ms);
                            p1 = ms.ToArray();
                        }
                    }
                    couponFromDB.Picture = p1;
                }
                couponFromDB.Discount = coupon.Discount;
                couponFromDB.MinimumAmount = coupon.Discount;
                couponFromDB.Name = coupon.Name;
                couponFromDB.CouponType = coupon.CouponType;
                couponFromDB.IsActive = coupon.IsActive;
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);    
            if(coupon==null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            var coupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();
            return View(nameof(Index));
        }
    }
}