﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.Extensions.Logging;
//using Jering.Vak.WebApplication.Models;
//using Jering.Vak.DatabaseInterface;
//using Microsoft.AspNetCore.Http.Authentication;
//using System.Data.SqlClient;
//using Jering.Vak.WebApplication.Utility;

//namespace Jering.Vak.WebApplication.Controllers
//{
//    [Authorize]
//    public class AccountController : Controller
//    {
//        private readonly MemberRepository _memberRepository;
//        private readonly SignInManager _signInManager;
//        //private readonly IEmailSender _emailSender;
//        //private readonly ILogger _logger;

//        public AccountController(
//            MemberRepository memberRepository, 
//            SignInManager signInManager  //,
//            //IEmailSender emailSender,
//            //ISmsSender smsSender,
//            //ILoggerFactory loggerFactory
//            )
//        {
//            _memberRepository = memberRepository;
//            _signInManager = signInManager;
//            //_emailSender = emailSender;
//            //_smsSender = smsSender;
//            //_logger = loggerFactory.CreateLogger<AccountController>();
//        }

//        //
//        // GET: /Account/Login
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Login(string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }

//        //
//        // POST: /Account/Login
//        //[HttpPost]
//        //[AllowAnonymous]
//        //[ValidateAntiForgeryToken]
//        //public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
//        //{
//        //    ViewData["ReturnUrl"] = returnUrl;
//        //    if (ModelState.IsValid)
//        //    {
//        //        // This doesn't count login failures towards account lockout
//        //        // To enable password failures to trigger account lockout, set lockoutOnFailure: true
//        //        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
//        //        if (result.Succeeded)
//        //        {
//        //            _logger.LogInformation(1, "User logged in.");
//        //            return RedirectToLocal(returnUrl);
//        //        }
//        //        if (result.RequiresTwoFactor)
//        //        {
//        //            return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
//        //        }
//        //        if (result.IsLockedOut)
//        //        {
//        //            _logger.LogWarning(2, "User account locked out.");
//        //            return View("Lockout");
//        //        }
//        //        else
//        //        {
//        //            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
//        //            return View(model);
//        //        }
//        //    }

//        //    // If we got this far, something failed, redisplay form
//        //    return View(model);
//        //}

//        //
//        // GET: /Account/Register
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult Register(string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }

//        //
//        // POST: /Account/Register
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            if (ModelState.IsValid)
//            {
//                Member member;

//                try
//                {
//                    member = await _memberRepository.CreateMemberAsync(model.Email, model.Password);
//                    //_logger.LogInformation(3, "User created a new account with password.");
//                }
//                catch (SqlException sqlException)
//                {
//                    // TODO: handle exceptions
//                    if (sqlException.Class == 0)
//                    {
//                        //AddErrors(result);
//                    }
//                    return View(model);
//                }

//                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
//                // Send an email with this link
//                //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
//                //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
//                //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
//                //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

//                await _signInManager.SignInAsync(member, new AuthenticationProperties { IsPersistent = true,  });
//                return RedirectToLocal(returnUrl);
//            }
//            else
//            {
//                // Model state not valid, return form
//                return View(model);
//            }
//        }

//        //
//        // POST: /Account/LogOff
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> LogOff()
//        {
//            await _signInManager.SignOutAsync();
//            _logger.LogInformation(4, "User logged out.");
//            return RedirectToAction(nameof(HomeController.Index), "Home");
//        }

//        // GET: /Account/ConfirmEmail
//        [HttpGet]
//        [AllowAnonymous]
//        public async Task<IActionResult> ConfirmEmail(string userId, string code)
//        {
//            if (userId == null || code == null)
//            {
//                return View("Error");
//            }
//            var user = await _memberRepository.FindByIdAsync(userId);
//            if (user == null)
//            {
//                return View("Error");
//            }
//            var result = await _memberRepository.ConfirmEmailAsync(user, code);
//            return View(result.Succeeded ? "ConfirmEmail" : "Error");
//        }

//        //
//        // GET: /Account/ForgotPassword
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult ForgotPassword()
//        {
//            return View();
//        }

//        //
//        // POST: /Account/ForgotPassword
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await _memberRepository.FindByNameAsync(model.Email);
//                if (user == null || !(await _memberRepository.IsEmailConfirmedAsync(user)))
//                {
//                    // Don't reveal that the user does not exist or is not confirmed
//                    return View("ForgotPasswordConfirmation");
//                }

//                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
//                // Send an email with this link
//                //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
//                //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
//                //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
//                //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
//                //return View("ForgotPasswordConfirmation");
//            }

//            // If we got this far, something failed, redisplay form
//            return View(model);
//        }

//        //
//        // GET: /Account/ForgotPasswordConfirmation
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult ForgotPasswordConfirmation()
//        {
//            return View();
//        }

//        //
//        // GET: /Account/ResetPassword
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult ResetPassword(string code = null)
//        {
//            return code == null ? View("Error") : View();
//        }

//        //
//        // POST: /Account/ResetPassword
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var user = await _memberRepository.FindByNameAsync(model.Email);
//            if (user == null)
//            {
//                // Don't reveal that the user does not exist
//                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
//            }
//            var result = await _memberRepository.ResetPasswordAsync(user, model.Code, model.Password);
//            if (result.Succeeded)
//            {
//                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
//            }
//            AddErrors(result);
//            return View();
//        }

//        //
//        // GET: /Account/ResetPasswordConfirmation
//        [HttpGet]
//        [AllowAnonymous]
//        public IActionResult ResetPasswordConfirmation()
//        {
//            return View();
//        }

//        //
//        // GET: /Account/SendCode
//        [HttpGet]
//        [AllowAnonymous]
//        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
//        {
//            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//            if (user == null)
//            {
//                return View("Error");
//            }
//            var userFactors = await _memberRepository.GetValidTwoFactorProvidersAsync(user);
//            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
//            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
//        }

//        //
//        // POST: /Account/SendCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> SendCode(SendCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View();
//            }

//            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//            if (user == null)
//            {
//                return View("Error");
//            }

//            // Generate the token and send it
//            var code = await _memberRepository.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
//            if (string.IsNullOrWhiteSpace(code))
//            {
//                return View("Error");
//            }

//            var message = "Your security code is: " + code;
//            if (model.SelectedProvider == "Email")
//            {
//                await _emailSender.SendEmailAsync(await _memberRepository.GetEmailAsync(user), "Security Code", message);
//            }
//            else if (model.SelectedProvider == "Phone")
//            {
//                await _smsSender.SendSmsAsync(await _memberRepository.GetPhoneNumberAsync(user), message);
//            }

//            return RedirectToAction(nameof(VerifyCode), new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
//        }

//        //
//        // GET: /Account/VerifyCode
//        [HttpGet]
//        [AllowAnonymous]
//        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
//        {
//            // Require that the user has already logged in via username/password or external login
//            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
//            if (user == null)
//            {
//                return View("Error");
//            }
//            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
//        }

//        //
//        // POST: /Account/VerifyCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            // The following code protects for brute force attacks against the two factor codes.
//            // If a user enters incorrect codes for a specified amount of time then the user account
//            // will be locked out for a specified amount of time.
//            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);
//            if (result.Succeeded)
//            {
//                return RedirectToLocal(model.ReturnUrl);
//            }
//            if (result.IsLockedOut)
//            {
//                _logger.LogWarning(7, "User account locked out.");
//                return View("Lockout");
//            }
//            else
//            {
//                ModelState.AddModelError(string.Empty, "Invalid code.");
//                return View(model);
//            }
//        }

//        #region Helpers

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError(string.Empty, error.Description);
//            }
//        }

//        private Task<Member> GetCurrentUserAsync()
//        {
//            return _memberRepository.GetUserAsync(HttpContext.User);
//        }

//        private IActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl))
//            {
//                return Redirect(returnUrl);
//            }
//            else
//            {
//                return RedirectToAction(nameof(HomeController.Index), "Home");
//            }
//        }

//        #endregion
//    }
//}
