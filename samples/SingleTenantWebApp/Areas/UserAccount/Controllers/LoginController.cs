﻿using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers {
    [AllowAnonymous]
    public class LoginController : Controller {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public LoginController(AuthenticationService authSvc) {
            this.userAccountService = authSvc.UserAccountService;
            this.authSvc = authSvc;
        }

        public ActionResult Index() {
            return View(new LoginInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginInputModel model) {
            if (ModelState.IsValid) {
                BrockAllen.MembershipReboot.UserAccount account;
                if (userAccountService.AuthenticateWithUsernameOrEmail(model.Username, model.Password, out account)) {
                    authSvc.SignIn(account);

                    if (account.RequiresTwoFactorAuthCodeToSignIn) {
                        return View("TwoFactorAuthCodeLogin");
                    }
                    if (account.RequiresTwoFactorCertificateToSignIn) {
                        return RedirectToAction("CertificateLogin");
                    }

                    if (account.RequiresAuthenticatorToSignIn) {
                        return View("TwoFactorAuthenticatorLogin");
                    }

                    if (account.RequiresStaticPinToSignIn) {
                        var pinPositions = account.TwoFactorPinPositions.Split(';');
                        var authModel = new TwoFactorAuthInputModel {
                            Code = "111111",
                            FirstPinPosition = string.Format("{0}{1}", pinPositions[0],pinPositions[0] =="1" ? "st" : pinPositions[0] == "2" ? "nd" : pinPositions[0] == "3" ? "rd" : "st"),
                            SecondPinPosition = pinPositions[1]
                        };
                        return View("TwoFactorStaticPin", authModel);
                    }


                    if (userAccountService.IsPasswordExpired(account)) {
                        return RedirectToAction("Index", "ChangePassword");
                    }

                    if (Url.IsLocalUrl(model.ReturnUrl)) {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                } else {
                    ModelState.AddModelError("", "Invalid Username or Password");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwoFactorAuthCodeLogin(string button, TwoFactorAuthInputModel model) {
            if (!User.HasUserID()) {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            if (button == "signin") {
                if (ModelState.IsValid) {
                    BrockAllen.MembershipReboot.UserAccount account;
                    if (userAccountService.AuthenticateWithCode(this.User.GetUserId(), model.Code, out account)) {
                        authSvc.SignIn(account);

                        if (userAccountService.IsPasswordExpired(account)) {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        if (Url.IsLocalUrl(model.ReturnUrl)) {
                            return Redirect(model.ReturnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    } else {
                        ModelState.AddModelError("", "Invalid Code");
                    }
                }
            }

            if (button == "resend") {
                this.userAccountService.SendTwoFactorAuthenticationCode(this.User.GetUserId());
            }

            return View("TwoFactorAuthCodeLogin", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwoFactorAuthenticatorLogin(TwoFactorAuthInputModel model) {
            if (!User.HasUserID()) {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }


            if (ModelState.IsValid) {
                BrockAllen.MembershipReboot.UserAccount account;
                if (userAccountService.AuthenticateWithAuthenticator(this.User.GetUserId(), model.Code, out account)) {
                    authSvc.SignIn(account);

                    if (userAccountService.IsPasswordExpired(account)) {
                        return RedirectToAction("Index", "ChangePassword");
                    }

                    if (Url.IsLocalUrl(model.ReturnUrl)) {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                } else {
                    ModelState.AddModelError("", "Invalid Code");
                }
            }
            
            return View("TwoFactorGoogleAuthLogin", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwoFactorStaticPin(TwoFactorAuthInputModel model) {
            if (!User.HasUserID()) {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }


            if (ModelState.IsValid) {
                BrockAllen.MembershipReboot.UserAccount account;
                if (userAccountService.AuthenticateWithStaticPin(this.User.GetUserId(), model.FirstPinCharacter, model.SecondPinCharacter, out account)) {
                    authSvc.SignIn(account);

                    if (userAccountService.IsPasswordExpired(account)) {
                        return RedirectToAction("Index", "ChangePassword");
                    }

                    if (Url.IsLocalUrl(model.ReturnUrl)) {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                } else {
                    ModelState.AddModelError("", "Invalid pin number");
                }
            }

            return View("TwoFactorStaticPin", model);
        }


        public ActionResult CertificateLogin() {
            if (Request.ClientCertificate != null &&
                Request.ClientCertificate.IsPresent &&
                Request.ClientCertificate.IsValid) {
                try {
                    var cert = new X509Certificate2(Request.ClientCertificate.Certificate);
                    BrockAllen.MembershipReboot.UserAccount account;

                    var result = false;
                    // we're allowing the use of certs for login and for two factor auth. normally you'd 
                    // do only one or the other, but for the sake of the sample we're allowing both.
                    if (User.Identity.IsAuthenticated) {
                        // this is when we're doing cert logins for two factor auth
                        result = this.authSvc.UserAccountService.AuthenticateWithCertificate(User.GetUserId(), cert, out account);
                    } else {
                        // this is when we're just doing certs to login (so no two factor auth)
                        result = this.authSvc.UserAccountService.AuthenticateWithCertificate(cert, out account);
                    }

                    if (result) {
                        this.authSvc.SignIn(account, AuthenticationMethods.X509);

                        if (userAccountService.IsPasswordExpired(account)) {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Invalid login");
                } catch (ValidationException ex) {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View();
        }
    }
}
