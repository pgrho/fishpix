using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace Shipwreck.FishPix.Models
{
    public class FishDbContext : DbContext
    {
        public DbSet<FishImageData> Images { get; set; }
    }
}