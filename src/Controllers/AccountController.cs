﻿using KMezzenger.DataAccess;
using KMezzenger.Models;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace KMezzenger.Controllers
{
    public class AccountController : Controller
    {
        public static ViewDataDictionary _forceLogoutUser;
        public static ViewDataDictionary ForceLogoutUser
        {
            get
            {
                if (_forceLogoutUser == null)
                    _forceLogoutUser = new ViewDataDictionary();
                return _forceLogoutUser;
            }
        }

        public ActionResult LogOn(string returnUrl)
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
                model.UserName = model.UserName.Trim().ToLower();
                if (!UserRepository.ValidateUser(model.UserName, model.Password))
                {
                    ViewBag.error = "Wrong username or password, please try again!";
                    return View(model);
                }

                FormsAuthentication.SetAuthCookie(model.UserName, true);
                ForceLogoutUser.Remove(model.UserName);

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

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(LogOnModel model)
        {
            model.UserName = model.UserName.Trim().ToLower();
            if (UserRepository.check_user_exist(model.UserName))
            {
                ViewBag.error = string.Format("Username [{0}] is already existed!", model.UserName);
                return View(model);
            }

            UserRepository.create_user(model.UserName, model.Password);
            ViewBag.inform = string.Format("User [{0}] is created successfully, please login!", model.UserName);

            return View("LogOn", model);
        }

        public ActionResult ResetPassword()
        {
            return View("Register");
        }
        [HttpPost]
        public ActionResult ResetPassword(LogOnModel model)
        {
            model.UserName = model.UserName.Trim().ToLower();
            if (!UserRepository.check_user_exist(model.UserName))
            {
                ViewBag.error = string.Format("Username [{0}] is not existed!", model.UserName);
                return View("Register", model);
            }

            UserRepository.reset_password(model.UserName, model.Password);
            ViewBag.inform = string.Format("Password was reset successfully for user [{0}], please login!", model.UserName);

            return View("LogOn", model);
        }

        public ActionResult ForceUserLogout(string username)
        {
            ForceLogoutUser[username] = true;

            ViewBag.inform = string.Format("Force user [{0}] logout successful!", username);
            return RedirectToAction("Index", "Home");
        }

    }
}
