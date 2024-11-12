using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using Microsoft.AspNet.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Orders
        public ActionResult Index(string Searchtext)
        {
            IEnumerable<Order> items = db.Orders.Where(x => x.Code == Searchtext);
            return View(items);
        }
        public ActionResult View(int id)
        {
            var item = db.Orders.Find(id);
            return View(item);
        }
        public ActionResult Partial_SanPham(int id)
        {
            var items = db.OrderDetails.Where(x => x.OrderId == id).ToList();
            return PartialView(items);
        }
        public ActionResult Refund()
        {
            return View();
        }
        public ActionResult ListOrders(string id)
        {
            id = User.Identity.GetUserId();
            if (id == null)
            {
                return View();
            }
            else
            {
                IEnumerable<Order> items = db.Orders.Where(x => x.IdUser == id);
                items = items.OrderByDescending(y => y.CreatedDate);
                return View(items);
            }
        }
        [HttpPost]
        public ActionResult UpdateTT(int id, int trangthai)
        {
            var item = db.Orders.Find(id);
            if (item != null)
            {
                db.Orders.Attach(item);
                item.TypePayment = trangthai;
                db.Entry(item).Property(x => x.TypePayment).IsModified = true;
                db.SaveChanges();
                return Json(new { message = "Thành công", Success = true });
            }
            return Json(new { message = "Thất bại!", Success = false });
        }
    }
}