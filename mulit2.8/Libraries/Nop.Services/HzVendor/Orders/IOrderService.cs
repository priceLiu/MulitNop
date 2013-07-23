using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderService
    {
        #region Orders

        /// <summary>
        /// get order vendorId
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>vendorId</returns>
        int GetVendorIdByOrderId(int orderId);

        // <summary>
        /// get order vendor
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>vendorId</returns>
        Vendor GetVendorByOrderId(int orderId);

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
        /// <param name="os">Order status; null to load all orders</param>
        /// <param name="ps">Order payment status; null to load all orders</param>
        /// <param name="ss">Order shippment status; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="vendorId">vendor id</param>
        /// <returns>Order collection</returns>
        IPagedList<Order> SearchOrders(DateTime? startTime, DateTime? endTime,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, string billingEmail,
            string orderGuid, int pageIndex, int pageSize, int vendorId);

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="vendorId">vendor Id</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payment collection</returns>
        IPagedList<RecurringPayment> SearchRecurringPayments(int customerId,
            int initialOrderId, OrderStatus? initialOrderStatus,
            int pageIndex, int pageSize, 
            int vendorId,
            bool showHidden = false);
        #endregion

        #region Return requests

        /// <summary>
        /// Search return requests
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all entries</param>
        /// <param name="orderProductVariantId">Order product variant identifier; null to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="vendorId">vendor Id</param>
        /// <returns>Return requests</returns>
        IPagedList<ReturnRequest> SearchReturnRequests(int customerId,
            int orderProductVariantId, ReturnRequestStatus? rs,
            int pageIndex, int pageSize
            ,int vendorId
            );

        #endregion

    }
}
