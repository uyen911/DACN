﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CosmeticsStore.Models;
using System.Diagnostics;
using System.Web.WebPages;
using System.Data.Entity.Infrastructure;

namespace CosmeticsStore.Controllers
{
    [Authorize]
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

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /Account/User
        public ActionResult Index()
        {

            var items = db.Users.ToList();
            return View(items);
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


        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/Image/" + file.FileName));
            return "/Content/Image/" + file.FileName;
        }

        [AllowAnonymous]
        public async Task<ActionResult> Profile(string id)
        {
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Name", "Name");
            var _user = await UserManager.FindByNameAsync(id);
            return View(_user);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Profile(ApplicationUser model, string role)
        {
            if (ModelState.IsValid)
            {
                var _user = await UserManager.FindByNameAsync(model.UserName);
                _user.FullName = model.FullName;
                _user.Phone = model.Phone;
                _user.Email = model.Email;
                _user.Images = model.Images;

                var result = await UserManager.UpdateAsync(_user);
                if (result.Succeeded)
                {

                    if (!string.IsNullOrEmpty(role))
                        UserManager.AddToRole(_user.Id, role);
                    return RedirectToAction("Profile", "Account");
                }
                AddErrors(result);
            }
            ViewBag.Role = new SelectList(db.Roles.ToList(), "Id", "Name");

            return View(model);
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {

            return PartialView();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Phone = model.Phone,
                    DateOfBirth = model.DateOfBirth,
                    Sex = model.Sex,

                };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return PartialView(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }



        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Gọi phương thức Challenge để bắt đầu quá trình đăng nhập bên ngoài (ở đây là đăng nhập Facebook)
            var redirectUrl = Url.Action("ExternalLoginCallbackAsync", "Account", new { ReturnUrl = returnUrl });
            return new ChallengeResult(provider, redirectUrl);
        }
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallbackAsync(string returnUrl)
        {
            try
            {
                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();

                if (loginInfo == null)
                {
                    Debug.WriteLine("can't receive logininfo");
                }

                ApplicationUser user = null; // Declare and initialize the 'user' variable

                if (loginInfo.Login.LoginProvider == "Google")
                {
                    // Lấy thông tin từ Google
                    var email = loginInfo.Email;
                    var fullName = loginInfo.DefaultUserName;

                    // Kiểm tra xem tài khoản người dùng đã tồn tại trong ứng dụng hay chưa
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    user = userManager.FindByEmail(email);

                    if (user == null)
                    {
                        int atIndex = email.IndexOf('@');
                        string userNameByEmail = email.Substring(0, atIndex);

                        // Tạo một tài khoản người dùng mới từ thông tin đăng nhập bên ngoài
                        var userSave = new ApplicationUser
                        {

                            UserName = userNameByEmail,
                            Email = email,
                            FullName = fullName,
                            Phone = "0911365447",
                            DateOfBirth = DateTime.Parse("2/1/2002"),
                            Sex = "nữ"
                        };

                        var result = await userManager.CreateAsync(userSave, "test");
                        if (!result.Succeeded)
                        {
                            Debug.WriteLine("sai thong tin");
                            // Xử lý lỗi khi tạo tài khoản không thành công
                            return RedirectToAction("Login");
                        }

                        // Đăng nhập thành công và tạo phiên làm việc cho người dùng
                        await SignInManager.SignInAsync(userSave, isPersistent: false, rememberBrowser: false);
                        // Chuyển hướng người dùng đến trang Home
                        return RedirectToAction("Index", "Home");
                    }
                }

                if (loginInfo.Login.LoginProvider == "Facebook")
                {
                    // Lấy thông tin từ Facebook
                    var email = loginInfo.Email;
                    var fullName = loginInfo.DefaultUserName;

                    // Kiểm tra xem tài khoản người dùng đã tồn tại trong ứng dụng hay chưa
                    var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    user = userManager.FindByEmail(email);
                    int atIndex = email.IndexOf('@');
                    string userNameByEmail = email.Substring(0, atIndex);

                    if (user == null)
                    {
                        // Tạo một tài khoản người dùng mới từ thông tin đăng nhập bên ngoài
                        var userSave = new ApplicationUser
                        {
                            UserName = userNameByEmail,
                            Email = email,
                            FullName = fullName,
                            Phone = "0911365447",
                            DateOfBirth = DateTime.Parse("2/1/2002"),
                            Sex = "nữ"
                        };

                        var result = await UserManager.CreateAsync(userSave, "123456Aa@");
                        if (!result.Succeeded)
                        {
                            Debug.WriteLine("sai thong tin");
                            // Xử lý lỗi khi tạo tài khoản không thành công
                            return RedirectToAction("Login");
                        }

                        // Đăng nhập thành công và tạo phiên làm việc cho người dùng
                        await SignInManager.SignInAsync(userSave, isPersistent: false, rememberBrowser: false);
                        // Chuyển hướng người dùng đến trang Home
                        return RedirectToAction("Index", "Home");
                    }
                }


                if (user != null)
                {
                    // Tài khoản đã tồn tại, đăng nhập thành công và chuyển hướng người dùng đến trang Home
                    var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    return RedirectToAction("Index", "Home");
                }
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                return RedirectToAction("Index", "Home");

            }
            catch (DbUpdateException ex)
            {
                // Ghi log chi tiết lỗi
                Debug.WriteLine("Lỗi Cập nhật CSDL: " + ex.Message);
                Debug.WriteLine("Inner Exception: " + ex.InnerException);

                // Xử lý lỗi hoặc chuyển hướng đến trang lỗi
                return RedirectToAction("Error");
            }
            catch (Exception ex)
            {
                // Ghi log thông tin về lỗi
                Debug.WriteLine("Lỗi: " + ex.Message);
                Debug.WriteLine("StackTrace: " + ex.StackTrace);

                // Xử lý lỗi theo ý của bạn, ví dụ: chuyển hướng đến một trang lỗi
                return RedirectToAction("Error");
            }
        }









        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
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
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        public ActionResult test()
        {
            return View();
        }
        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
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

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}