using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Payments.SwissBitcoinPay.Models
{
    public record ConfigurationModel : BaseNopModel
    {

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.ApiUrl")]
        [Url]
        [Required]
        public string ApiUrl { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.ApiKey")]
        [Required]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.ApiSecret")]
        [Required]
        public string ApiSecret { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.AcceptOnChain")]
        public bool AcceptOnChain { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        [NopResourceDisplayName("Plugins.Payments.SwissBitcoinPay.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        public bool OverrideForStore { get; set; }


    }

}