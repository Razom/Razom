using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DataModel;
using Razom.Models;

namespace Razom.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult Parse()
        {
            ServiceReference1.Service1Client proxy = new ServiceReference1.Service1Client();
            proxy.runParce();
            proxy.Close();
            return RedirectToAction("Index","Home");
        }

        [Authorize]
        public ActionResult TestConnection()
        {
            ServiceReference1.Service1Client proxy = new ServiceReference1.Service1Client();
            ViewBag.Message = proxy.getResponse();
            proxy.Close();
            return View();
        }

        [ChildActionOnly]
        public ActionResult PanelButton()
        {
            if (HttpContext.Request.Cookies["role"] != null && User.Identity.IsAuthenticated)
            {
                string role = HttpContext.Request.Cookies["role"].Value;
                if (role == "admin")
                {
                    return PartialView();
                }
            }
            return null;
        }
    }
}
