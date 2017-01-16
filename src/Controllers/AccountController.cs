using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KMezzenger.Models;
using KMezzenger.DataAccess;
using System.Web.Security;

namespace KMezzenger.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult LogOn(string ReturnUrl)
        {
            if (Request.IsAjaxRequest())
                return PartialView();
            else
                return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (!UserRepository.ValidateUser(model.UserName, model.Password))
                {
                    ViewBag.error = "Wrong username or password, please try again!";
                    return View(model);
                }

                FormsAuthentication.SetAuthCookie(model.UserName, true);

                if (!String.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        public ActionResult LogOff()
        {
            //write system log for logout and keep IP-Computer Name
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

    }
}
