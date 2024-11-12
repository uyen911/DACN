using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Controllers
{
    public class AddressController : Controller
    {
        // GET: Address
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            string id = User.Identity.GetUserId();
            var item = db.AddressBooks.Where(x => x.UserID.Contains(id));
            return View(item);
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddressBook model)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                model.UserID = id;
                db.AddressBooks.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Edit(int id)
        {
            var item = db.AddressBooks.Find(id);
            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddressBook model)
        {
            if (ModelState.IsValid)
            {
                string id = User.Identity.GetUserId();
                model.UserID = id;
                db.AddressBooks.Attach(model);
                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var item = db.AddressBooks.Find(id);
            if (item != null)
            {
                db.AddressBooks.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult Default(int id)
        {
            string userId = User.Identity.GetUserId();
            var items = db.AddressBooks.Where(x => x.UserID == userId);
            foreach (var item in items)
            {
                item.IsDefault = false;
            }
            var add = db.AddressBooks.Find(id);
            add.IsDefault = true;
            db.SaveChanges();
            return Json(new { SuccesS = true });
        }
    }
}