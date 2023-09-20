using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.SwissBitcoinPay.Components
{
    public class PaymentSwissBitcoinPayViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.SwissBitcoinPay/Views/PaymentInfo.cshtml");
        }
    }
}
