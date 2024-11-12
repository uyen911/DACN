using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class SettingSystemViewModel
    {
        public string SettingTitle { get; set; }
        public string SettingLogo { get; set; }
        public string SettingHotline { get; set; }
        public string SettingEmail { get; set; }
        public string SettingTitleSeo { get; set; }
        public string SettingDesSeo { get; set; }
        public string SettingKeySeo { get; set; }
        public string UrlVNP { get; set; }
        public string ReturnUrlVNP { get; set; }
        public string TmnCodeVNP { get; set; }
        public string HashSecretVNP { get; set; }
        public string EndpointMomo { get; set; }
        public string PartnerCodeMomo { get; set; }
        public string AccessKeyMomo { get; set; }
        public string SerectkeyMomo { get; set; }
        public string OrderInfoMomo { get; set; }
        public string ReturnUrlMomo { get; set; }
        public string NotifyurlMomo { get; set; }
    }
}