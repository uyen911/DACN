﻿using CosmeticsStore.Models;
using CosmeticsStore.Models.EF;
using CKFinder.Settings;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI;
using Microsoft.AspNet.Identity.Owin;
using static CosmeticsStore.Controllers.AccountController;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace CosmeticsStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Account
        public ActionResult Index()
        {

            var items = db.Users.ToList();
            return View(items);
        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/Image/" + file.FileName));
            return "/Content/Image/" + file.FileName;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var user = await UserManager.FindAsync(model.UserName, model.Password);
                        user.LockoutEndDateUtc = DateTime.Now;
                        UserManager.Update(user);
                    }

                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    {
                        var user = await UserManager.FindAsync(model.UserName, model.Password);
                        if (user.EmailConfirmed == false)
                        {
                            user.LockoutEnabled = true;
                            UserManager.Update(user);
                        }
                    }
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Create()
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Images = model.Images,
                    DateOfBirth = model.DateOfBirth,
                    Sex = model.Sex,
                    LockoutEnabled = model.LockoutEnabled

                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, model.Role);
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    /*string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");*/

                    return RedirectToAction("Index", "Account");
                }
                AddErrors(result);
            }

            ViewBag.Role = new SelectList(db.Roles.ToList(), "Id", "Name");
            // If we got this far, something failed, redisplay form
            return View(model);
        }
        // GET: /Account/Register
        [AllowAnonymous]
        public async Task<ActionResult> Edit(string id)
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            var _user = await UserManager.FindByIdAsync(id);

            var roles = await UserManager.GetRolesAsync(_user.Id);

            ViewBag.Roles = roles;

            return View(_user);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ApplicationUser model, string role)
        {
            if (ModelState.IsValid)
            {
                var _user = await UserManager.FindByIdAsync(model.Id);
                _user.FullName = model.FullName;
                _user.UserName = model.UserName;
                _user.Phone = model.Phone;
                _user.Email = model.Email;
                _user.Images = model.Images;
                _user.DateOfBirth = model.DateOfBirth;
                _user.Sex = model.Sex;
                _user.LockoutEnabled = model.LockoutEnabled;



                var result = await UserManager.UpdateAsync(_user);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(role))
                    {
                        var roles = await UserManager.GetRolesAsync(_user.Id);
                        await UserManager.RemoveFromRolesAsync(_user.Id, roles.ToArray());
                        await UserManager.AddToRoleAsync(_user.Id, role);
                    }
                    return RedirectToAction("Index", "Account");
                }
                AddErrors(result);
            }



            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateTT(string id, bool trangthai)
        {
            var item = db.Users.Find(id);
            if (item != null)
            {
                db.Users.Attach(item);
                item.LockoutEnabled = trangthai;
                db.Entry(item).Property(x => x.LockoutEnabled).IsModified = true;
                db.SaveChanges();
                return Json(new { message = "Thành công", Success = true });
            }
            return Json(new { message = "Thất bại!", Success = false });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var item = db.Users.Find(id);
            if (item != null)
            {

                db.Users.Remove(item);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false });

        }




        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //        // AccountController.cs
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await UserManager.FindByNameAsync(model.Email);
        //        if (user != null)
        //        {
        //            await UserManager.SendPasswordResetTokenAsync(user.Id, "Default");
        //            return RedirectToAction("ForgotPasswordConfirmation", "Account");
        //        }
        //    }

        //    // Nếu không tìm thấy người dùng hoặc có lỗi, hãy hiển thị thông báo lỗi
        //    ModelState.AddModelError("", "Không tìm thấy người dùng hoặc có lỗi.");
        //    return View(model);
        //}
        //// ForgotPasswordViewModel.cs
        //public class ForgotPasswordViewModel
        //{
        //    [Required]
        //    [Display(Name = "Email")]
        //    public string Email { get; set; }
        //}

    }
}