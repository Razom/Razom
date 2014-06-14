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
            //ServiceReference1.Service1Client proxy = new ServiceReference1.Service1Client();
            //ViewBag.Message = proxy.getResponse();
            //proxy.Close();
            List<ShortPlace> list = new List<ShortPlace>
            {
             new ShortPlace{ Name="vania", City="Поїхали"},
             new ShortPlace{ Name="vasia", City = "Я ще не зібрався"},
             new ShortPlace{ Name="vania", City="Ти довго ще?"},
             new ShortPlace{ Name="vania", City="Ти довго ще?"},
             new ShortPlace{ Name="vasia", City="Зараз уже йду..."},
             new ShortPlace{ Name="vasia", City="аааа аааа аааааа аааааа ааааааа аааааа аааа ааааа аааааа ааааааа аааа ааааа аааа аааа ааа ааа"}
            };
            return View(list);
        }

        [Authorize]
        public ActionResult Parse()
        {
            ServiceReference1.Service1Client proxy = new ServiceReference1.Service1Client();
            proxy.runParce();
            proxy.Close();
            return RedirectToAction("Index","Home");
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
