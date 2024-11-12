using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CosmeticsStore.Controllers
{
    public class ChatController : Controller
    {
        // GET: Chat
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;
            Debug.WriteLine(userId);
            return View();
        }
    }
}