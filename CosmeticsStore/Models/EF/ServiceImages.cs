using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models.EF
{
    [Table("tb_ServiceImage")]
    public class ServiceImages
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Image { get; set; }
        public bool IsDefault { get; set; }
        public virtual Service Service { get; set; }
    }
}