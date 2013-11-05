using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class TwoFactorAuthController : Controller
    {
        UserAccountService userAccountService;

        public TwoFactorAuthController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        public ActionResult Index()
        {
            var acct = userAccountService.GetById(this.User.GetUserId());
            return View(acct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(TwoFactorAuthMode mode)
        {
            try
            {
                this.userAccountService.ConfigureTwoFactorAuthentication(this.User.GetUserId(), mode);
                
                ViewData["Message"] = "Update Success";
                
                var acct = userAccountService.GetById(this.User.GetUserId());

                if (mode == TwoFactorAuthMode.Authenticator)
                    return View("AuthenticatorCode", acct);

                if (mode == TwoFactorAuthMode.StaticPin)
                    return View("SetPin", acct);

                return View("Index", acct);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            
            return View("Index", userAccountService.GetById(this.User.GetUserId()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetPin(string newPin) {
            try {
                this.userAccountService.SetStaticPin(this.User.GetUserId(), newPin);

                ViewData["Message"] = "Update Success";

                var acct = userAccountService.GetById(this.User.GetUserId());
                return View("SetPin", acct);
            } catch (ValidationException ex) {
                ModelState.AddModelError("", ex.Message);
            }

            return View("SetPin", userAccountService.GetById(this.User.GetUserId()));
        }
    }
}
