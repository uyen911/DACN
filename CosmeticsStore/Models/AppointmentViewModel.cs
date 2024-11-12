using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
    }

}