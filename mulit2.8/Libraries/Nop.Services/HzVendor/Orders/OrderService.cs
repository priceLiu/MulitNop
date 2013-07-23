//add by hz full page
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderService : IOrderService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVendor> _productVendorRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="opvRepository">Order product variant repository</param>
        /// <param name="orderNoteRepository">Order note repository</param>
        /// <param name="pvRepository">Product variant repository</param>
        /// <param name="recurringPaymentRepository">Recurring payment repository</param>
        /// <param name="customerRepository">Customer repository</param>
        /// <param name="returnRequestRepository">Return request repository</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="productRepository">Product repository</param>//add by hz
        public OrderService(IRepository<Order> orderRepository,
            IRepository<OrderProductVariant> opvRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<ProductVariant> pvRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<Customer> customerRepository,
            IRepository<ReturnRequest> returnRequestRepository,
            IEventPublisher eventPublisher
            , IRepository<Product> productRepository//add by hz
            , IRepository<ProductVendor> productVendorRepository
            )
        {
            _orderRepository = orderRepository;
            _opvRepository = opvRepository;
            _orderNoteRepository = orderNoteRepository;
            _pvRepository = pvRepository;
            _recurringPaymentRepository = recurringPaymentRepository;
            _customerRepository = customerRepository;
            _returnRequestRepository = returnRequestRepository;
            _eventPublisher = eventPublisher;
            _productRepository = productRepository;//add by hz
            _productVendorRepository = productVendorRepository;//add by hz
        }

        #endregion

        #region Methods

        #region Orders

        /// <summary>
        /// get order vendorId
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>vendorId</returns>
        public virtual int GetVendorIdByOrderId(int orderId)
        {
            if (orderId == 0)
                return 0;
            var query = from ps in _productVendorRepository.Table
                        join p in _productRepository.Table on ps.ProductId equals p.Id
                        join pv in _pvRepository.Table on p.Id equals pv.ProductId
                        join opv in _opvRepository.Table on pv.Id equals opv.ProductVariantId
                        join o in _orderRepository.Table on opv.OrderId equals o.Id
                        where o.Id == orderId
                        select ps;
            try 
            { 
                return query.FirstOrDefault().VendorId;
            }
            catch
            {
                return 0;
            }
            
        }

        /// <summary>
        /// get order vendor
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>vendorId</returns>
        public virtual Vendor GetVendorByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;
            var query = from ps in _productVendorRepository.Table
                        join p in _productRepository.Table on ps.ProductId equals p.Id
                        join pv in _pvRepository.Table on p.Id equals pv.ProductId
                        join opv in _opvRepository.Table on pv.Id equals opv.ProductVariantId
                        join o in _orderRepository.Table on opv.OrderId equals o.Id
                        where o.Id == orderId
                        select ps.Vendor;

            return query.FirstOrDefault();

        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
        /// <param name="os">Order status; null to load all orders</param>
        /// <param name="ps">Order payment status; null to load all orders</param>
        /// <param name="ss">Order shippment status; null to load all orders</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all orders.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="storId">vendor id</param>
        /// <returns>Order collection</returns>
        public virtual IPagedList<Order> SearchOrders(DateTime? startTime, DateTime? endTime,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, string billingEmail, 
            string orderGuid, int pageIndex, int pageSize, int vendorId)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var query = _orderRepository.Table;
            //add by hz
            if (vendorId > 0)
            {
                query = (from o in _orderRepository.Table
                         join opv in _opvRepository.Table on o.Id equals opv.OrderId
                         join pv in _pvRepository.Table on opv.ProductVariantId equals pv.Id
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId
                         where pvendor.VendorId == vendorId
                         select o).Distinct() ;
            }
            //end by hz
            if (startTime.HasValue)
                query = query.Where(o => startTime.Value <= o.CreatedOnUtc);
            if (endTime.HasValue)
                query = query.Where(o => endTime.Value >= o.CreatedOnUtc);
            if (orderStatusId.HasValue)
                query = query.Where(o => orderStatusId.Value == o.OrderStatusId);
            if (paymentStatusId.HasValue)
                query = query.Where(o => paymentStatusId.Value == o.PaymentStatusId);
            if (shippingStatusId.HasValue)
                query = query.Where(o => shippingStatusId.Value == o.ShippingStatusId);
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            var orders = query.ToList();
            
            //filter by GUID. Filter in BLL because EF doesn't support casting of GUID to string
            if (!String.IsNullOrEmpty(orderGuid))
                orders = orders.FindAll(o => o.OrderGuid.ToString().ToLowerInvariant().Contains(orderGuid.ToLowerInvariant()));

            return new PagedList<Order>(orders, pageIndex, pageSize);
        }

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
        public virtual IPagedList<RecurringPayment> SearchRecurringPayments(int customerId,
            int initialOrderId, OrderStatus? initialOrderStatus,
            int pageIndex, int pageSize,
            int vendorId,
            bool showHidden = false)
        {
            int? initialOrderStatusId = null;
            if (initialOrderStatus.HasValue)
                initialOrderStatusId = (int)initialOrderStatus.Value;

            var query1 = from rp in _recurringPaymentRepository.Table
                         join c in _customerRepository.Table on rp.InitialOrder.CustomerId equals c.Id
                         where
                         (!rp.Deleted) &&
                         (showHidden || !rp.InitialOrder.Deleted) &&
                         (showHidden || !c.Deleted) &&
                         (showHidden || rp.IsActive) &&
                         (customerId == 0 || rp.InitialOrder.CustomerId == customerId) &&
                         (initialOrderId == 0 || rp.InitialOrder.Id == initialOrderId) &&
                         (!initialOrderStatusId.HasValue || initialOrderStatusId.Value == 0 || rp.InitialOrder.OrderStatusId == initialOrderStatusId.Value)
                         select rp.Id;

            var query2 = from rp in _recurringPaymentRepository.Table
                         where query1.Contains(rp.Id)
                         orderby rp.StartDateUtc, rp.Id
                         select rp;

            var recurringPayments = new PagedList<RecurringPayment>(query2, pageIndex, pageSize);
            return recurringPayments;
        }
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
        public virtual IPagedList<ReturnRequest> SearchReturnRequests(int customerId,
            int orderProductVariantId, ReturnRequestStatus? rs,
            int pageIndex, int pageSize
            ,int vendorId
            )
        {
            var query = _returnRequestRepository.Table;

            //add by hz
            if (vendorId > 0)
            {
                query = from rr in _returnRequestRepository.Table
                        join opv in _opvRepository.Table on rr.OrderProductVariantId equals opv.ProductVariantId
                        join pv in _pvRepository.Table on opv.ProductVariantId equals pv.Id
                        join p in _productRepository.Table on pv.ProductId equals p.Id
                        join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId
                        where pvendor.VendorId == vendorId
                        select rr;
            }
            //end by hz

            if (customerId > 0)
                query = query.Where(rr => customerId == rr.CustomerId);
            if (rs.HasValue)
            {
                int returnStatusId = (int)rs.Value;
                query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
            }
            if (orderProductVariantId > 0)
                query = query.Where(rr => rr.OrderProductVariantId == orderProductVariantId);

            query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);


            var returnRequests = new PagedList<ReturnRequest>(query, pageIndex, pageSize);
            return returnRequests;
        }

        #endregion
        
        #endregion
    }
}
