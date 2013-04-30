//add by hz full page
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Helpers;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order report service
    /// </summary>
    public partial class OrderReportService : IOrderReportService
    {
        #region Fields

        private readonly IRepository<ProductVendor> _productVendorRepository;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="opvRepository">Order product variant repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="productVariantRepository">Product variant repository</param>
        /// <param name="dateTimeHelper">Datetime helper</param>
        /// <param name="productService">Product service</param>
        public OrderReportService(IRepository<Order> orderRepository,
            IRepository<OrderProductVariant> opvRepository,
            IRepository<Product> productRepository,
            IRepository<ProductVariant> productVariantRepository,
            IDateTimeHelper dateTimeHelper, IProductService productService
            ,IRepository<ProductVendor> productVendorRepository
            )
        {
            this._orderRepository = orderRepository;
            this._opvRepository = opvRepository;
            this._productRepository = productRepository;
            this._productVariantRepository = productVariantRepository;
            this._dateTimeHelper = dateTimeHelper;
            this._productService = productService;
            this._productVendorRepository = productVendorRepository;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="os">Order status</param>
        /// <param name="ps">Payment status</param>
        /// <param name="ss">Shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="ignoreCancelledOrders">A value indicating whether to ignore cancelled orders</param>
        /// <returns>Result</returns>
        public virtual OrderAverageReportLine GetOrderAverageReportLine(OrderStatus? os,
            PaymentStatus? ps, ShippingStatus? ss, DateTime? startTimeUtc, DateTime? endTimeUtc,
            string billingEmail,
            int vendorId,
            bool ignoreCancelledOrders = false)
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
                         join pv in _productVariantRepository.Table on opv.ProductVariantId equals pv.Id
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId 
                         where pvendor.VendorId == vendorId
                         select o).Distinct();
            }
            //end by hz
            query = query.Where(o => !o.Deleted);
            if (ignoreCancelledOrders)
            {
                int cancelledOrderStatusId = (int)OrderStatus.Cancelled;
                query = query.Where(o => o.OrderStatusId != cancelledOrderStatusId);
            }
            if (orderStatusId.HasValue)
                query = query.Where(o => o.OrderStatusId == orderStatusId.Value);
            if (paymentStatusId.HasValue)
                query = query.Where(o => o.PaymentStatusId == paymentStatusId.Value);
            if (shippingStatusId.HasValue)
                query = query.Where(o => o.ShippingStatusId == shippingStatusId.Value);
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));

			var item = (from oq in query
						group oq by 1 into result
						select new { OrderCount = result.Count(), OrderTaxSum = result.Sum(o => o.OrderTax), OrderTotalSum = result.Sum(o => o.OrderTotal) }
					   ).Select(r => new OrderAverageReportLine(){ SumTax = r.OrderTaxSum, CountOrders=r.OrderCount, SumOrders = r.OrderTotalSum}).FirstOrDefault();

			item = item ?? new OrderAverageReportLine() { CountOrders = 0, SumOrders = decimal.Zero, SumTax = decimal.Zero };
            return item;
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="os">Order status</param>
        /// <param name="vendorId">Vendor id</param>
        /// <returns>Result</returns>
        public virtual OrderAverageReportLineSummary OrderAverageReport(OrderStatus os, int vendorId)
        {
            var item = new OrderAverageReportLineSummary();
            item.OrderStatus = os;

            DateTime nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            TimeZoneInfo timeZone = _dateTimeHelper.CurrentTimeZone;

            //today
            DateTime t1 = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            if (!timeZone.IsInvalidTime(t1))
            {
                DateTime? startTime1 = _dateTimeHelper.ConvertToUtcTime(t1, timeZone);
                DateTime? endTime1 = null;
                var todayResult = GetOrderAverageReportLine(os, null,null, startTime1, endTime1, null,
                    vendorId
                    );
                item.SumTodayOrders = todayResult.SumOrders;
                item.CountTodayOrders = todayResult.CountOrders;
            }
            //week
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DateTime today = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            DateTime t2 = today.AddDays(-(today.DayOfWeek - fdow));
            if (!timeZone.IsInvalidTime(t2))
            {
                DateTime? startTime2 = _dateTimeHelper.ConvertToUtcTime(t2, timeZone);
                DateTime? endTime2 = null;
                var weekResult = GetOrderAverageReportLine(os, null, null, startTime2, endTime2, null
                    ,vendorId
                    );
                item.SumThisWeekOrders = weekResult.SumOrders;
                item.CountThisWeekOrders = weekResult.CountOrders;
            }
            //month
            DateTime t3 = new DateTime(nowDt.Year, nowDt.Month, 1);
            if (!timeZone.IsInvalidTime(t3))
            {
                DateTime? startTime3 = _dateTimeHelper.ConvertToUtcTime(t3, timeZone);
                DateTime? endTime3 = null;
                var monthResult = GetOrderAverageReportLine(os, null, null, startTime3, endTime3, null
                    ,vendorId
                    );
                item.SumThisMonthOrders = monthResult.SumOrders;
                item.CountThisMonthOrders = monthResult.CountOrders;
            }
            //year
            DateTime t4 = new DateTime(nowDt.Year, 1, 1);
            if (!timeZone.IsInvalidTime(t4))
            {
                DateTime? startTime4 = _dateTimeHelper.ConvertToUtcTime(t4, timeZone);
                DateTime? endTime4 = null;
                var yearResult = GetOrderAverageReportLine(os, null, null, startTime4, endTime4, null
                    ,vendorId
                    );
                item.SumThisYearOrders = yearResult.SumOrders;
                item.CountThisYearOrders = yearResult.CountOrders;
            }
            //all time
            DateTime? startTime5 = null;
            DateTime? endTime5 = null;
            var allTimeResult = GetOrderAverageReportLine(os, null, null, startTime5, endTime5, null
                ,vendorId
                );
            item.SumAllTimeOrders = allTimeResult.SumOrders;
            item.CountAllTimeOrders = allTimeResult.CountOrders;

            return item;
        }

        /// <summary>
        /// Get best sellers report
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="groupBy">1 - group by product variants, 2 - group by products</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Result</returns>
        public virtual IList<BestsellersReportLine> BestSellersReport(int vendorId,DateTime? startTime,
            DateTime? endTime, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,  
            int billingCountryId = 0,
            int recordsToReturn = 5, int orderBy = 1, int groupBy = 1, bool showHidden = false)
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


            var query1 = from opv in _opvRepository.Table
                         join o in _orderRepository.Table on opv.OrderId equals o.Id
                         join pv in _productVariantRepository.Table on opv.ProductVariantId equals pv.Id
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                         (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                         (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                         (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                         (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                         (!o.Deleted) &&
                         (!p.Deleted) &&
                         (!pv.Deleted) &&
                         (billingCountryId == 0 || o.BillingAddress.CountryId == billingCountryId) &&
                         (showHidden || p.Published) &&
                         (showHidden || pv.Published)
                         select opv;

            //add by hz
            if (vendorId > 0)
            {
                query1 = from opv in query1
                         join pv in _productVariantRepository.Table on opv.ProductVariantId equals pv.Id
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId
                         where (pvendor.VendorId == vendorId)
                         select opv;
            }
            //end by hz

            var query2 = groupBy == 1 ?
                //group by product variants
                from opv in query1
                group opv by opv.ProductVariantId into g
                select new
                {
                    EntityId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity),
                }
                :
                //group by products
                from opv in query1
                group opv by opv.ProductVariant.ProductId into g
                select new
                {
                    EntityId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity),
                }
                ;

            switch (orderBy)
            {
                case 1:
                    {
                        query2 = query2.OrderByDescending(x => x.TotalQuantity);
                    }
                    break;
                case 2:
                    {
                        query2 = query2.OrderByDescending(x => x.TotalAmount);
                    }
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            if (recordsToReturn != 0 && recordsToReturn != int.MaxValue)
                query2 = query2.Take(recordsToReturn);

            var result = query2.ToList().Select(x =>
            {
                var reportLine = new BestsellersReportLine()
                {
                    EntityId = x.EntityId,
                    TotalAmount = x.TotalAmount,
                    TotalQuantity = x.TotalQuantity
                };
                return reportLine;
            }).ToList();

            return result;
        }

        /// <summary>
        /// Gets a list of product variants that were never sold
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="vendorId">Vendor id</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product variants</returns>
        public virtual IPagedList<ProductVariant> ProductsNeverSold(DateTime? startTime,
            DateTime? endTime, int pageIndex, int pageSize, 
            int vendorId,
            bool showHidden = false)
        {
            //this inner query should retrieve all purchased order product varint identifiers
            var query1 = (from opv in _opvRepository.Table
                          join o in _orderRepository.Table on opv.OrderId equals o.Id
                          where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                                (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                                (!o.Deleted)
                          select opv.ProductVariantId).Distinct();

            var query2 = from pv in _productVariantRepository.Table
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         where (!query1.Contains(pv.Id)) &&
                               (!p.Deleted) &&
                               (!pv.Deleted) &&
                               (showHidden || p.Published) &&
                               (showHidden || pv.Published)
                         select pv;
            //add by hz
            if (vendorId > 0)
            {
                query2 = from pv in query2
                         join p in _productRepository.Table on pv.ProductId equals p.Id
                         join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId
                         where (pvendor.VendorId == vendorId)
                         select pv;
            }
            //end by hz

            //only distinct products (group by ID)
            //if we use standard Distinct() method, then all fields will be compared (low performance)
            //it'll not work in SQL Server Compact when searching products by a keyword)
            var query3 = from pv in query2
                         group pv by pv.Id
                         into pvGroup
                         orderby pvGroup.Key
                         select pvGroup.FirstOrDefault();

            query3 = query3.OrderBy(x => x.Id);

            var productVariants = new PagedList<ProductVariant>(query3, pageIndex, pageSize);
            return productVariants;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="vendorId">Vendor Id</param>
        /// <returns>Result</returns>
        public virtual decimal ProfitReport(OrderStatus? os,
            PaymentStatus? ps, ShippingStatus? ss, DateTime? startTimeUtc, DateTime? endTimeUtc,
            string billingEmail
            ,int vendorId
            )
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
            //We cannot use String.IsNullOrEmpty(billingEmail) in SQL Compact
            bool dontSearchEmail = String.IsNullOrEmpty(billingEmail);
            var query = from opv in _opvRepository.Table
                        join o in _orderRepository.Table on opv.OrderId equals o.Id
                        join pv in _productVariantRepository.Table on opv.ProductVariantId equals pv.Id
                        join p in _productRepository.Table on pv.ProductId equals p.Id
                        where (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                              (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                              (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                              (!o.Deleted) &&
                              (!p.Deleted) &&
                              (!pv.Deleted) &&
                              (dontSearchEmail || (o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail)))
                        select new { opv, pv };

            //add by hz
            if (vendorId > 0)
            {
                query = from pv in query
                         join p in _productRepository.Table on pv.pv.ProductId equals p.Id
                         join pvendor in _productVendorRepository.Table on p.Id equals pvendor.ProductId
                         where (pvendor.VendorId == vendorId)
                         select pv;
            }
            //end by hz

            var productCost = Convert.ToDecimal(query.Sum(o => (decimal?) o.pv.ProductCost * o.opv.Quantity));
            var reportSummary = GetOrderAverageReportLine(os, ps, ss, startTimeUtc, endTimeUtc, billingEmail);
            var profit = reportSummary.SumOrders - reportSummary.SumTax - productCost;
            return profit;
        }

        #endregion
    }
}
