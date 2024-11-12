using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class AppointmentWithCustomerViewModel
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public String Date { get; set; }
        public string Status { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public string Phone { get; set; }
    }
}