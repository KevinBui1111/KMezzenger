﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using KMezzenger.Models;
using KMezzenger.Controllers;
using System.Web.Security;

namespace KMezzenger
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            GlobalHost.HubPipeline.RequireAuthentication();
            GlobalHost.HubPipeline.AddModule(new MyHubPipelineModule());
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true };
            //app.MapSignalR(hubConfiguration);
            RouteTable.Routes.MapHubs(hubConfiguration);

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (/*HttpContext.Current.Session != null &&*/ AccountController.ForceLogoutUser.ContainsKey(User.Identity.Name))
            {
                //HttpContext.Current.Session.Abandon();
                FormsAuthentication.SignOut();
                //HttpContext.Current.Response.Redirect("~/Home");
            }
        }

    }
}