using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,StaffProductPostNew")]
    public class ProductImageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Admin/ProductImage
        public ActionResult Index(int id)
        {
            ViewBag.ProductId = id;
            var items = db.ProductImages.Where(x => x.ProductId == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int productId, string url)
        {
            db.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                Image = url,
                IsDefault = false
            });
            db.SaveChanges();
            return Json(new { Success = true });
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ProductImages.Find(id);
            db.ProductImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult Default(int productId, int id)
        {
            var items = db.ProductImages.Where(x => x.ProductId == productId);
            foreach (var item in items)
            {
                item.IsDefault = false;
            }
            var add = db.ProductImages.Find(id);
            add.IsDefault = true;
            db.SaveChanges();
            return Json(new { SuccesS = true });
        }

    }
}