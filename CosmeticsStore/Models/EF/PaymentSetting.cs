using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace CosmeticsStore.Models.EF
{
    [Table("tb_PaymentSetting")]

    public class PaymentSetting : CommonAbstract
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [StringLength(150)]
        public string UrlVNP { get; set; }
        [StringLength(150)]
        public string ReturnUrlVNP { get; set; }
        [StringLength(150)]
        public string TmnCodeVNP { get; set; }
        [StringLength(150)]
        public string HashSecretVNP { get; set; }
        [StringLength(150)]
        public string EndpointMomo { get; set; }
        [StringLength(150)]
        public string PartnerCodeMomo { get; set; }
        [StringLength(150)]
        public string AccessKeyMomo { get; set; }
        [StringLength(150)]
        public string SerectkeyMomo { get; set; }
        [StringLength(150)]
        public string OrderInfoMomo { get; set; }
        [StringLength(150)]
        public string ReturnUrlMomo { get; set; }
        [StringLength(150)]
        public string NotifyurlMomo { get; set; }
    }
}
