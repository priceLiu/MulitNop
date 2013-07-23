//add  by hz full page
using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Payments;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order processing service interface
    /// </summary>
    public partial interface IOrderProcessingService
    {
        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>	
        PlaceOrderResult PlaceOrder(VendorOrder vendorOrder);
    }
}
