using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Events;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Shipment service
    /// </summary>
    public partial class ShipmentService : IShipmentService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductVendor> _productVendorRepository;
        private readonly IRepository<ProductVariant> _pvRepository;
        private readonly IRepository<OrderProductVariant> _opvRepository;
        private readonly IRepository<Order> _orderRepository;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shipmentRepository">Shipment repository</param>
        /// <param name="sopvRepository">Shipment order product variant repository</param>
        /// <param name="eventPublisher">Event published</param>
        public ShipmentService(IRepository<Shipment> shipmentRepository,
            IRepository<ShipmentOrderProductVariant> sopvRepository,
            IEventPublisher eventPublisher,
            IRepository<Product> productRepository,
            IRepository<ProductVendor> productVendorRepository,
            IRepository<ProductVariant> pvRepository,
            IRepository<OrderProductVariant> opvRepository,
            IRepository<Order> _orderRepository
            )
        {
            this._shipmentRepository = shipmentRepository;
            this._sopvRepository = sopvRepository;
            _eventPublisher = eventPublisher;
            this._productRepository = productRepository;
            this._productVendorRepository = productVendorRepository;
            this._pvRepository = pvRepository;
            this._opvRepository = opvRepository;
            this._orderRepository = _orderRepository;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="createdFrom">Created date from; null to load all records</param>
        /// <param name="createdTo">Created date to; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        public virtual IPagedList<Shipment> GetAllShipments(DateTime? createdFrom, DateTime? createdTo, 
            int pageIndex, int pageSize, int vendorId)
        {

            //var query = from ps in _productVendorRepository.Table
            //            join p in _productRepository.Table on ps.ProductId equals p.Id
            //            join pv in _pvRepository.Table on p.Id equals pv.ProductId
            //            join opv in _opvRepository.Table on pv.Id equals opv.ProductVariantId
            //            join o in _orderRepository.Table on opv.OrderId equals o.Id
            //            where o.Id == orderId
            //            select ps;

            var query = _shipmentRepository.Table;

            if (vendorId > 0)
            {
                query = from s in _shipmentRepository.Table
                        join o in _orderRepository.Table on s.OrderId equals o.Id
                        join opv in _opvRepository.Table on o.Id equals opv.OrderId
                        join pv in _pvRepository.Table on opv.ProductVariantId equals pv.ProductId
                        join p in _productRepository.Table on pv.ProductId equals p.Id
                        //join ps in _productVendorRepository.Table on p.Id equals ps.ProductId
                        //where ps.VendorId== vendorId 
                        //add  by hz to do
                        select s;
            }
            //var query = _shipmentRepository.Table;

            if (createdFrom.HasValue)
                query = query.Where(s => createdFrom.Value <= s.CreatedOnUtc);
            if (createdTo.HasValue)
                query = query.Where(s => createdTo.Value >= s.CreatedOnUtc);
            query = query.Where(s => s.Order != null && !s.Order.Deleted);
            query = query.OrderByDescending(s => s.CreatedOnUtc);

            var shipments = new PagedList<Shipment>(query, pageIndex, pageSize);
            return shipments;
        }

      

        #endregion
    }
}
