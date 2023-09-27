using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SwissBitcoinPay
{
    /// <summary>
    /// Represents settings of SwissBitcoinPay payment plugin
    /// </summary>
    public class SwissBitcoinPaySettings : ISettings
    {
        /// <summary>
        /// The url of your the Swiss Bitcoin Pay API
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Your Swiss Bitcoin Pay API Key given in your Swiss Bitcoin Pay dashboard
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// The Api Secret value set in your Swiss Bitcoin Pay dashboard
        /// </summary>
        public string ApiSecret { get; set; }

        public bool AcceptOnChain { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

    }
}