using CosmeticsStore.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin, StaffCSKH")]
    public class ChatController : Controller
    {
        // GET: Admin/Chat
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult chat()
        {
            return View();
        }

        // Hàm lấy thông tin của user từ cơ sở dữ liệu
        public JsonResult GetUserById(string userId)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                var name = user.FullName;
                var image = user.Images;
                return Json(new { name, image }, JsonRequestBehavior.AllowGet);
            }
            return Json(null);
        }


    }
}