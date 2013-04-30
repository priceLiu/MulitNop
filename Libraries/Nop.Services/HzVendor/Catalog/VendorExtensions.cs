//add by hz full page
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class VendorExtensions
    {
        /// <summary>
        /// Returns a ProductVendor that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>A ProductVendor that has the specified values; otherwise null</returns>
        public static ProductVendor FindProductVendor(this IList<ProductVendor> source,
            int productId, int vendorId)
        {
            foreach (var productVendor in source)
                if (productVendor.ProductId == productId && productVendor.VendorId == vendorId)
                    return productVendor;

            return null;
        }

        /// <summary>
        /// Returns a VendorCustomer that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>A VendorCustomer that has the specified values; otherwise null</returns>
        public static VendorCustomer FindVendorCustomer(this IList<VendorCustomer> source,
            int customerId, int vendorId)
        {
            foreach (var vendorCustomer in source)
                if (vendorCustomer.CustomerId == customerId && vendorCustomer.VendorId == vendorId)
                    return vendorCustomer;

            return null;
        }

    }
}
