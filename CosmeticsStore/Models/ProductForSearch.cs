using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class ProductForSearch
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }

        public string Description{ get; set; }

        public string ProductLink { get; set; }
    }
}