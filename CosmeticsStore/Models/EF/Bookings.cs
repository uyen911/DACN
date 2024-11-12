using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models.EF
{
    [Table("tb_Bookings")]
    public class Bookings : CommonAbstract
    {
        public Bookings()
        {
            this.BookingDetails = new HashSet<BookingDetails>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        
        [Required]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Status { get; set; }

        public string Date { get; set; }
        public string serviceId { get; set; }
        public virtual ICollection<BookingDetails> BookingDetails { get; set; }
    }
}