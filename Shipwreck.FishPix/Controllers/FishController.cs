using Shipwreck.FishPix.Models;
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
    public class FishController : Controller
    {
        public async Task<ActionResult> Search(
                                   int start = 1,
                                   string name = null, MatchOperator nameOperator = MatchOperator.Equal,
                                   string species = null, MatchOperator speciesOperator = MatchOperator.Equal,
                                   string japaneseFamily = null, MatchOperator japaneseFamilyOperator = MatchOperator.Equal,
                                   string family = null, MatchOperator familyOperator = MatchOperator.Equal)
        {
            var sb = new StringBuilder("http://fishpix.kahaku.go.jp/fishimage/search?START=");
            sb.Append(start);

            AppendQueryParameter(sb, "JPN_NAME", name, nameOperator);
            AppendQueryParameter(sb, "JPN_FAMILY", japaneseFamily, japaneseFamilyOperator);
            AppendQueryParameter(sb, "FAMILY", family, familyOperator);
            AppendQueryParameter(sb, "SPECIES", species, speciesOperator);

            // =&=&JPN_NAME=%83A%83%86&=&LOCALITY=&FISH_Y=&FISH_M=&FISH_D=&PERSON=&PHOTO_ID=&JPN_FAMILY_OPT=1&FAMILY_OPT=1&JPN_NAME_OPT=0&SPECIES_OPT=1&LOCALITY_OPT=1&PERSON_OPT=1&PHOTO_ID_OPT=2

            using (var wc = new WebClient())
            {
                var t = await wc.DownloadStringTaskAsync(sb.ToString());

                var matches = Regex.Matches(t, @"src=""\.\.\/photos\/[A-Z]{2}[0-9]{4}\/([A-Z]{2}[0-9]{7})AI\.jpg""");
                var jd = Regex.Matches(t, @" class=""result"">([^>]+)<").Cast<Match>().ToDictionary(_ => _.Index, _ => _.Groups[1].Value);
                var ld = Regex.Matches(t, @" class=""resultHelvetica"">([^>]+)<").Cast<Match>().ToDictionary(_ => _.Index, _ => _.Groups[1].Value);

                var l = new List<FishImage>(matches.Count);

                var ub = new UriBuilder(Request.Url);

                foreach (Match m in matches)
                {
                    var id = m.Groups[1].Value;
                    ub.Path = Url.Action("Image", new { id = id });
                    var f = new FishImage()
                    {
                        Id = id,
                        ImageUrl = ub.ToString(),
                        JapaneseName = jd.Where(_ => _.Key > m.Index).OrderBy(_ => _.Key).FirstOrDefault().Value,
                        LatinName = ld.Where(_ => _.Key > m.Index).OrderBy(_ => _.Key).FirstOrDefault().Value
                    };

                    l.Add(f);
                }

                return Json(new FishImageResult() { Items = l }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> Image(string id)
        {
            var f = await FishImageData.GetOrCreateAsync(id);

            if (f.CroppedImageData != null)
            {
                return File(f.CroppedImageData, "image/jpeg");
            }

            return Redirect(f.OriginalUrl);
        }

        public async Task<ActionResult> OriginalImage(string id)
        {
            var f = await FishImageData.GetOrCreateAsync(id);

            return Redirect(f.OriginalUrl);
        }

        private static void AppendQueryParameter(StringBuilder sb, string key, string value, MatchOperator @operator)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append('&').Append(key).Append('=');
                AppendPercentEncoded(sb, value);
                sb.Append('&').Append(key).Append("_OPT=").Append((int)@operator);
            }
        }

        private static void AppendPercentEncoded(StringBuilder sb, string query)
        {
            var bytes = Encoding.GetEncoding(932).GetBytes(query);

            foreach (var b in bytes)
            {
                var c = (char)b;

                switch (c)
                {
                    #region case [A-Z]:
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                    case 'G':
                    case 'H':
                    case 'I':
                    case 'J':
                    case 'K':
                    case 'L':
                    case 'M':
                    case 'N':
                    case 'O':
                    case 'P':
                    case 'Q':
                    case 'R':
                    case 'S':
                    case 'T':
                    case 'U':
                    case 'V':
                    case 'W':
                    case 'X':
                    case 'Y':
                    case 'Z':
                    #endregion

                    #region case [a-z]:
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                    #endregion

                    #region case [0-9]:
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    #endregion

                    case '!':
                    case '(':
                    case ')':
                    case '_':
                    case '-':
                    case '*':
                    case '.':
                        sb.Append(c);
                        break;
                    case ' ':
                        sb.Append('+');
                        break;
                    default:
                        sb.Append('%');
                        sb.Append(b.ToString("X2"));
                        break;
                }
            }
        }
    }
}