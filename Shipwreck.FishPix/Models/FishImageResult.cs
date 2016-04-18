using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shipwreck.FishPix.Models
{
    public class FishImageResult
    {
        public IReadOnlyList<FishImage> Items { get; set; }
    }
}