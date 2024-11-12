using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Controllers
{
    public class ContactController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Contact
        public ActionResult Index()
        {
            var items = db.Branchs.OrderBy(x => x.Id).ToList();
            return View(items);
        }
    }
}