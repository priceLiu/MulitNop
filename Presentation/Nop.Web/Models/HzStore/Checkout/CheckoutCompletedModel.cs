//add by hz page
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Checkout
{
    public partial class CheckoutCompletedModel : BaseNopModel
    { 
        public CheckoutCompletedModel()
        {
            CheckoutVendorOrders = new List<CheckoutCompletedVendorOrderModel>();
        }

        public string[] OrderIds { get; set; }
        public IList<CheckoutCompletedVendorOrderModel> CheckoutVendorOrders { get; set; }


        public int OrderId { get; set; }
    }

    public partial class CheckoutCompletedVendorOrderModel : BaseNopModel
    {
        public Vendor Vendor { get; set; }
        public int OrderId { get; set; }
    }
}