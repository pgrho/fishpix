using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Shipwreck.FishPix.Controllers
{
    public class FishImage
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string JapaneseName { get; set; }
        public string LatinName { get; set; }
    }
}