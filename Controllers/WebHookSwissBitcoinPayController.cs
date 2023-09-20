using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.SwissBitcoinPay.Services;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using ILogger = Nop.Services.Logging.ILogger;

namespace Nop.Plugin.Payments.SwissBitcoinPay.Controllers
{
    public class WebHookSwissBitcoinPayController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;

        public WebHookSwissBitcoinPayController(IOrderService orderService,
            ISettingService settingService,
            ILogger logger)
                {
                    _settingService = settingService;
                    _orderService = orderService;
                    _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> Process([FromHeader(Name = "sbp-sig")] string SwissBtcPaySig)
        {
            string jsonStr = "";
            int step = 0;
            try
            {
                if (SwissBtcPaySig.IsNullOrEmpty())
                {
                    await _logger.ErrorAsync("Secret key not set");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                step++;
                jsonStr = await new StreamReader(Request.Body).ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(jsonStr);

                step++;
                var SwissBtcPaySecret = SwissBtcPaySig.Split('=')[1];

                step++;
                string Description = jsonData.description;
                var tblDescription = Description.Split(" | ");
                step++;
                string OrderGuid = tblDescription[1].Split(" : ")[1];
                step++;
                int StoreID = Convert.ToInt32(tblDescription[2].Split(" : ")[1]);
                step++;

                if (String.IsNullOrEmpty(OrderGuid) || StoreID == 0)
                {
                    await _logger.ErrorAsync("Missing fields in request");
                    return StatusCode(StatusCodes.Status422UnprocessableEntity);
                }
                step++;

                bool IsPaid = jsonData.isPaid;
                bool IsExpired = jsonData.isExpired;


                step++;
                var swissBitcoinPaySettings = await _settingService.LoadSettingAsync<SwissBitcoinPaySettings>(StoreID);

                if (!SwissBitcoinPayService.CheckSecretKey(swissBitcoinPaySettings.ApiSecret, jsonStr, SwissBtcPaySecret))
                    throw (null);
                var order = await _orderService.GetOrderByGuidAsync(new Guid(OrderGuid)) ?? throw (null);
                step++;

                if (IsPaid) order.PaymentStatus = PaymentStatus.Paid;
                if (IsExpired)
                {
                    if (order.PaymentStatus != PaymentStatus.Paid)
                                                order.PaymentStatus = PaymentStatus.Voided;
                }

                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "PaymentStatus: " + order.PaymentStatus.ToString(),
                    DisplayToCustomer = true,
                    CreatedOnUtc = DateTime.UtcNow
                });

                await _orderService.UpdateOrderAsync(order);
                return StatusCode(StatusCodes.Status200OK); //new EmptyResult() ;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"{step.ToString()} - {jsonStr} - {ex.Message}", ex);
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

    }
}
