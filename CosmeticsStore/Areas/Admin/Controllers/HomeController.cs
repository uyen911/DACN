using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using PagedList;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, StaffOrder, StaffBooking, StaffProductPostNew, StaffCSKH")]
    public class HomeController : Controller
    {
        // GET: Admin/Home
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            ViewBag.Orders = db.Orders.Count();
            ViewBag.Products = db.Products.Count();
            ViewBag.Posts = db.Posts.Count();
            ViewBag.News = db.News.Count();
            IEnumerable<Order> items = db.Orders.OrderByDescending(x => x.CreatedDate);
            return View(items);
        }
    }
}