using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using CosmeticsStore.Models;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web;

namespace CosmeticsStore
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext<UserManager<ApplicationUser>>(() => new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())));


            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            app.UseFacebookAuthentication(
            appId: "543134474648469",
            appSecret: "4eea17514cd5578b6ce4f0e349509748");


            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = "43395991015-8ir03i80bbm6afvg8r0tpvo2dde83dvp.apps.googleusercontent.com",
                ClientSecret = "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",
                Provider = new GoogleOAuth2AuthenticationProvider
                {
                    OnAuthenticated = async context =>
                    {
                        // Extract the email address from the Google profile
                        var email = context.Identity.FindFirstValue(ClaimTypes.Email);

                        // Check if a user with the same email address exists in the application
                        var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                        var user = await userManager.FindByEmailAsync(email);

                        if (user == null)
                        {
                            // If the user doesn't exist, create a new ApplicationUser and set its properties
                            user = new ApplicationUser { UserName = email, Email = email };

                            // Create the user in the application's database
                            var result = await userManager.CreateAsync(user);
                            if (result.Succeeded)
                            {
                                // Add the external login to the newly created user
                                var owinContext = context.OwinContext;
                                var authenticationManager = owinContext.Authentication;
                                var externalIdentity = authenticationManager.GetExternalIdentity(DefaultAuthenticationTypes.ExternalCookie);
                                var provider = externalIdentity?.AuthenticationType;

                                result = await userManager.AddLoginAsync(user.Id, new UserLoginInfo(provider, externalIdentity?.FindFirstValue(ClaimTypes.NameIdentifier)));

                                if (result.Succeeded)
                                {
                                    // Set the newly created user as the current logged-in user
                                    authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, await user.GenerateUserIdentityAsync(userManager));
                                }
                            }
                        }
                        context.Response.Redirect("/home/index");
                    }
                }
            };


            app.UseGoogleAuthentication(googleOptions);
        }
    }
}
