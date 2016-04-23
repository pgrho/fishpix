using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Shipwreck.FishPix
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "FishImage",
                url: "Fish/Image/{id}.jpg",
                defaults: new { controller = "Fish", action = "Image", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "FishOriginalImage",
                url: "Fish/OriginalImage/{id}.jpg",
                defaults: new { controller = "Fish", action = "OriginalImage", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Fish", action = "Search", id = UrlParameter.Optional }
            );
        }
    }
}
