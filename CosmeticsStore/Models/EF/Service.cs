using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CosmeticsStore.Models.EF
{
    [Table("tb_Service")]
    public class Service : CommonAbstract
    {
        public Service()
        {
            this.ServiceImage = new HashSet<ServiceImages>();
            this.BookingDetails = new HashSet<BookingDetails>();
        }
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string Title { get; set; }
        public string Alias { get; set; }
        [StringLength(50)]
        public string ServiceCode { get; set; }
        public string Description { get; set; }
        [AllowHtml]
        public string Detail { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string SeoKeywords { get; set; }
        public bool IsActive { get; set; }
        public int ServiceCategoryId { get; set; }
        public virtual ICollection<ServiceImages> ServiceImage { get; set; }
        public virtual ICollection<BookingDetails> BookingDetails { get; set; }
    }
}
