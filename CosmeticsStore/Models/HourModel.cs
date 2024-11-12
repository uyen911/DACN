using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class HourModel
    {
        public DateTime Hour { get; set; }
        public bool IsPastTime { get; set; }    
    }
}