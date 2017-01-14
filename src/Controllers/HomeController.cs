using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KMezzenger.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            string a = Request.ServerVariables["LOGON_USER"];
            bool b = User.Identity.IsAuthenticated;
            return View();
        }
        public ActionResult Chat()
        {
            return View();
        }
    }
}
