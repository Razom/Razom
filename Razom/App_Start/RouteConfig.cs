using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Razom
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute(
                null,
                "Trip/ConfirmAddPlace/{id}/{uid}",
                new { Controller = "Trip", action = "ConfirmAddPlace", id = UrlParameter.Optional, uid = UrlParameter.Optional }
                );
            routes.MapRoute(
                null,
                "Trip/ConfirmAddUser/{id}/{uid}",
                new { Controller = "Trip", action = "ConfirmAddUser", id = UrlParameter.Optional, uid = UrlParameter.Optional }
                );
            routes.MapRoute(
                null,
                "Place/Show/{id}/{page}",
                new { Controller = "Place", action = "Show", id = UrlParameter.Optional, page = UrlParameter.Optional }
                );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}