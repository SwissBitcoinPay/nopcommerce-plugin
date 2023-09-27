using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.SwissBitcoinPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NUglify.Helpers;

namespace Nop.Plugin.Payments.SwissBitcoinPay.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class PaymentSwissBitcoinPayController : BasePaymentController
    {
        #region Fields
        
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentSwissBitcoinPayController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swissBitcoinPaySettings = await _settingService.LoadSettingAsync<SwissBitcoinPaySettings>(storeScope);

            var model = new ConfigurationModel
            {
                ApiUrl = swissBitcoinPaySettings.ApiUrl.IfNullOrWhiteSpace(""),
                ApiKey = swissBitcoinPaySettings.ApiKey.IfNullOrWhiteSpace(""),
                ApiSecret = swissBitcoinPaySettings.ApiSecret.IfNullOrWhiteSpace(""),
                AcceptOnChain = swissBitcoinPaySettings.AcceptOnChain,
                AdditionalFee = swissBitcoinPaySettings.AdditionalFee,
                AdditionalFeePercentage = swissBitcoinPaySettings.AdditionalFeePercentage,
                ActiveStoreScopeConfiguration = storeScope
            };

            return View("~/Plugins/Payments.SwissBitcoinPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var swissBitcoinPaySettings = await _settingService.LoadSettingAsync<SwissBitcoinPaySettings>(storeScope);

            //save settings
            swissBitcoinPaySettings.ApiUrl = model.ApiUrl.Trim();
            swissBitcoinPaySettings.ApiKey = model.ApiKey.Trim();
            swissBitcoinPaySettings.ApiSecret = model.ApiSecret.Trim();
            swissBitcoinPaySettings.AcceptOnChain = model.AcceptOnChain;
            swissBitcoinPaySettings.AdditionalFee = model.AdditionalFee;
            swissBitcoinPaySettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.ApiUrl, model.OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.ApiKey, model.OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.ApiSecret, model.OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.AcceptOnChain, model.OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.AdditionalFee, model.OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(swissBitcoinPaySettings, x => x.AdditionalFeePercentage, model.OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion


    }
}