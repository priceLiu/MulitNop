using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Shipping;

namespace Nop.Services.Shipping
{
    /// <summary>
    /// Shipment service interface
    /// </summary>
    public partial interface IShipmentService
    {

        /// <summary>
        /// Search shipments
        /// </summary>
        /// <param name="createdFrom">Created date from; null to load all records</param>
        /// <param name="createdTo">Created date to; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Customer collection</returns>
        IPagedList<Shipment> GetAllShipments(DateTime? createdFrom, DateTime? createdTo, 
            int pageIndex, int pageSize, int vendorId);
    }
}
