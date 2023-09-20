using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.SwissBitcoinPay.Components;
using Nop.Plugin.Payments.SwissBitcoinPay.Models;
using Nop.Plugin.Payments.SwissBitcoinPay.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Stores;

namespace Nop.Plugin.Payments.SwissBitcoinPay
{
    public class SwissBitcoinPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Properties
        public bool SupportCapture => false;

        public bool SupportPartiallyRefund => false;

        public bool SupportRefund => false;

        public bool SupportVoid => false;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;
        #endregion

        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IStoreService _storeService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly ILanguageService _languageService;
        private readonly SwissBitcoinPaySettings _swissBitcoinPaySettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public SwissBitcoinPayPaymentProcessor(CurrencySettings currencySettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            ISettingService settingService,
            IStoreService storeService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            SwissBitcoinPaySettings swissBitcoinPaySettings,
            IServiceProvider serviceProvider,
            IWebHelper webHelper,
            ILogger logger)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _currencyService = currencyService;
            _webHelper = webHelper;
            _customerService = customerService;
            _storeService = storeService;
            _currencySettings = currencySettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _languageService = languageService;
            _swissBitcoinPaySettings = swissBitcoinPaySettings;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new SwissBitcoinPaySettings
            {
                ApiUrl = "https://api.swiss-bitcoin-pay.ch"
            });
            ;

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.SwissBitcoinPay.AdditionalFee"] = "Additional fee",
                ["Plugins.Payments.SwissBitcoinPay.AdditionalFee.Hint"] = "The additional fee.",
                ["Plugins.Payments.SwissBitcoinPay.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugins.Payments.SwissBitcoinPay.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugins.Payments.SwissBitcoinPay.Instructions"] = "Enter your data below to configure this plugin :",
                ["Plugins.Payments.SwissBitcoinPay.PaymentMethodDescription"] = "Pay in bitcoins with Swiss Bitcoin Pay",
                ["Plugins.Payments.SwissBitcoinPay.PaymentMethodDescription2"] = "After completing the order you will be redirected to Swiss Bitcoin Pay, where you can make the Bitcoin payment for your order.",
                ["Plugins.Payments.SwissBitcoinPay.APIUrl"] = "Swiss Bitcoin Pay API Url",
                ["Plugins.Payments.SwissBitcoinPay.ApiKey"] = "API Key",
                ["Plugins.Payments.SwissBitcoinPay.ApiKey.Hint"] = "The API Key value generated in your Swiss Bitcoin Pay account",
                ["Plugins.Payments.SwissBitcoinPay.ApiSecret"] = "API Secret",
                ["Plugins.Payments.SwissBitcoinPay.ApiSecret.Hint"] = "The Api Secret value generated in your Swiss Bitcoin Pay account",
                ["Plugins.Payments.SwissBitcoinPay.AcceptOnChain"] = "Accept OnChain payments",
                ["Plugins.Payments.SwissBitcoinPay.AcceptOnChain.Hint"] = "Accept Bitcoin OnChain payments?",
            });

            await base.InstallAsync();
        }

        #endregion    


        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<SwissBitcoinPaySettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Payments.SwissBitcoinPay");

            await base.UninstallAsync();
        }

        Task<CancelRecurringPaymentResult> IPaymentMethod.CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        Task<bool> IPaymentMethod.CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(true);
        }

        Task<CapturePaymentResult> IPaymentMethod.CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _swissBitcoinPaySettings.AdditionalFee, _swissBitcoinPaySettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentSwissBitcoinPay/Configure";
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.SwissBitcoinPay.PaymentMethodDescription");
        }

        public Type GetPublicViewComponent()
        {
            return typeof(PaymentSwissBitcoinPayViewComponent);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return Task.FromResult(false);
        }

        Task<ProcessPaymentResult> IPaymentMethod.ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }

        Task<RefundPaymentResult> IPaymentMethod.RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            throw new NotImplementedException();
        }

        Task<IList<string>> IPaymentMethod.ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            if (form["Agree"] == "false")
            {
                var checkAgree = Task.Run(() => _localizationService.GetResourceAsync("Plugins.Payments.SwissBitcoinPay.CheckAgree")).Result;
                warnings.Add(checkAgree);
            }

            return Task.FromResult<IList<string>>(warnings);

        }

        Task<VoidPaymentResult> IPaymentMethod.VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            throw new NotImplementedException();
        }


        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;

            // implement process payment
            try
            {

                var myStore = await _storeService.GetStoreByIdAsync(processPaymentRequest.StoreId);
                var myCustomer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
                var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
                var lang = await _languageService.GetLanguageByIdAsync(myStore.DefaultLanguageId);
                var langCode = (lang == null) ? "en" : lang.UniqueSeoCode;

                var apiService = new SwissBitcoinPayService();
                result.AuthorizationTransactionResult = apiService.CreateInvoice(_swissBitcoinPaySettings, new PaymentDataModel()
                {
                    CurrencyCode = currency.CurrencyCode,
                    Amount = processPaymentRequest.OrderTotal,
                    BuyerEmail = "" + myCustomer.Email,
                    BuyerName = myCustomer.FirstName + " " + myCustomer.LastName,
                    OrderID = processPaymentRequest.OrderGuid.ToString(),
                    StoreID = processPaymentRequest.StoreId,
                    CustomerID = processPaymentRequest.CustomerId,
                    Description = "From " + myStore.Name,
                    RedirectionURL = myStore.Url + "checkout/completed",
                    Lang = langCode,
                    WebHookURL = myStore.Url + "WebHookSwissBitcoinPay/Process"
                });

            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                result.AddError(ex.Message);
            }

            return await Task.FromResult(result);
        }


        public Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Pending)
            {
                //postProcessPaymentRequest.Order.HasNewPaymentNotification = true;
                // Specify redirection URL here if your provider is of type PaymentMethodType.Redirection.
                // Core redirects to this URL automatically.
                var accessor = _serviceProvider.GetService<IHttpContextAccessor>();
                accessor.HttpContext.Response.Redirect(postProcessPaymentRequest.Order.AuthorizationTransactionResult);
            }
            return Task.CompletedTask;
        }
    }
}