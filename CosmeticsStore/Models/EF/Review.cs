using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models.EF
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public float StarRating { get; set; }
        public DateTime Date { get; set; }
        public String CustomerId { get; set; }
        public int ProductId { get; set; }
    }
}