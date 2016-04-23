using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Shipwreck.FishPix.Models
{
    public class FishImageData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(16)]
        [Column(TypeName = "VARCHAR")]
        [Index(IsUnique = true)]
        public string FishPixId { get; set; }

        [StringLength(255)]
        [Column(TypeName = "VARCHAR")]
        [Index(IsUnique = true)]
        public string OriginalUrl { get; set; }

        public short OriginalWidth { get; set; }

        public short OriginalHeight { get; set; }

        public short CroppedWidth { get; set; }

        public short CroppedHeight { get; set; }

        public byte[] CroppedImageData { get; set; }

        public static async Task<FishImageData> GetOrCreateAsync(string fishPixId)
        {
            FishImageData f;
            using (var db = new FishDbContext())
            {
                f = await db.Images.FirstOrDefaultAsync(_ => _.FishPixId == fishPixId);
                if (f != null)
                {
                    return f;
                }
            }

            f = new FishImageData();
            f.FishPixId = fishPixId;
            f.OriginalUrl = $"http://fishpix.kahaku.go.jp/photos/{fishPixId.Substring(0, 6)}/{fishPixId}AF.jpg";

            var r = WebRequest.Create(f.OriginalUrl);

            var res = await r.GetResponseAsync();

            using (var bmp = (Bitmap)Image.FromStream(res.GetResponseStream()))
            {
                f.OriginalWidth = (short)bmp.Width;
                f.OriginalHeight = (short)bmp.Height;

                var rect = GetBounds(bmp);
                f.CroppedWidth = (short)rect.Width;
                f.CroppedHeight = (short)rect.Height;

                var codec = ImageCodecInfo.GetImageEncoders().First(_ => _.MimeType == "image/jpeg");
                var saveParam = new EncoderParameters(1);
                saveParam.Param[0] = new EncoderParameter(Encoder.Quality, 99L);

                if (f.OriginalWidth != f.CroppedWidth || f.OriginalHeight != f.CroppedHeight)
                {
                    using (var other = new Bitmap(rect.Width, rect.Height))
                    {
                        using (var g = Graphics.FromImage(other))
                        {
                            g.DrawImage(bmp, new Rectangle(0, 0, other.Width, other.Height), rect, GraphicsUnit.Pixel);
                        }

                        using (var ms = new MemoryStream())
                        {
                            other.Save(ms, codec, saveParam);
                            f.CroppedImageData = ms.ToArray();
                        }
                    }
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, codec, saveParam);
                        f.CroppedImageData = ms.ToArray();
                    }
                }
            }

            using (var db = new FishDbContext())
            {
                db.Images.Add(f);
                await db.SaveChangesAsync();
            }

            return f;
        }

        unsafe private static Rectangle GetBounds(Bitmap bmp)
        {
            int bs;

            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    bs = 4;
                    break;

                case PixelFormat.Format24bppRgb:
                    bs = 3;
                    break;

                default:
                    using (var other = new Bitmap(bmp.Width, bmp.Height))
                    using (var g = Graphics.FromImage(other))
                    {
                        g.DrawImage(bmp, new PointF(0, 0));
                        return GetBounds(other);
                    }
            }

            var ow = bmp.Width;
            var oh = bmp.Height;
            var bd = bmp.LockBits(new Rectangle(0, 0, ow, oh), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var st = bd.Stride;

            int lx, ly, ux, uy;

            var p = (byte*)bd.Scan0;

            for (ly = 0; ly < oh; ly++)
            {
                for (var x = 0; x < ow; x++)
                {
                    var i = ly * st + bs * x;
                    if (!IsBackground(p[i++], p[i++], p[i++]))
                    {
                        goto BREAK_LY;
                    }
                }
            }
            BREAK_LY:

            for (uy = oh - 1; uy >= 0; uy--)
            {
                for (var x = 0; x < ow; x++)
                {
                    var i = uy * st + bs * x;
                    if (!IsBackground(p[i++], p[i++], p[i++]))
                    {
                        goto BREAK_UY;
                    }
                }
            }
            BREAK_UY:

            for (lx = 0; lx < ow; lx++)
            {
                for (var y = 0; y < oh; y++)
                {
                    var i = y * st + bs * lx;
                    if (!IsBackground(p[i++], p[i++], p[i++]))
                    {
                        goto BREAK_LX;
                    }
                }
            }
            BREAK_LX:

            for (ux = ow - 1; ux >= 0; ux--)
            {
                for (var y = 0; y < oh; y++)
                {
                    var i = y * st + bs * ux;
                    if (!IsBackground(p[i++], p[i++], p[i++]))
                    {
                        goto BREAK_UX;
                    }
                }
            }
            BREAK_UX:
            ;

            bmp.UnlockBits(bd);

            if (lx < ux && ly < uy)
            {
                return new Rectangle(lx, ly, ux - lx + 1, uy - ly + 1);
            }
            return new Rectangle(0, 0, ow, oh);
        }

        private static bool IsBackground(byte b, byte g, byte r)
            => b > 248 && g > 248 && r > 248;
    }
}