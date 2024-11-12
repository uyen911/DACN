using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models.EF
{
    [Table("tb_Branchs")]
    public class Branchs : CommonAbstract
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Title { get; set; }
        [StringLength(150)]
        public string Alias { get; set; }
        public string Location { get; set; }
        [StringLength(250)]
        public string Image { get; set; }
        [StringLength(150)]
        public string Phone { get; set; }
        [StringLength(150)]
        public string Map { get; set; }
    }
}