using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,StaffBooking")]
    public class ServiceImagesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index(int id)
        {
            ViewBag.ServiceId = id;
            var items = db.ServiceImages.Where(x => x.ServiceId == id).ToList();
            return View(items);
        }

        [HttpPost]
        public ActionResult AddImage(int serviceId, string url)
        {
            db.ServiceImages.Add(new ServiceImages
            {
                ServiceId = serviceId,
                Image = url,
                IsDefault = false
            });
            db.SaveChanges();
            return Json(new { Success = true });
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.ServiceImages.Find(id);
            db.ServiceImages.Remove(item);
            db.SaveChanges();
            return Json(new { success = true });
        }
        [HttpPost]
        public ActionResult Default(int id)
        {
            var item = db.ServiceImages.Find(id);
            if (item.IsDefault)
            {
                item.IsDefault = false;
            }
            else
            {
                item.IsDefault = true;
            }

            db.SaveChanges();
            return Json(new { SuccesS = true });
        }

    }
}