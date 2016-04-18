using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Shipwreck.FishPix.Models
{
    public enum MatchOperator
    {
        Equal = 0,
        Contains = 1,
        StartsWith = 2,
        EndsWith = 3
    }
}