//add by hz full page
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial interface IVendorService
    {
        /// <summary>
        /// Deletes a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void DeleteVendor(Vendor vendor);

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendor collection</returns>
        IList<Vendor> GetAllVendors(int customerVendorId = 0,bool showHidden = false);

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendor collection</returns>
        IList<Vendor> GetAllVendors(string vendorName, int customerVendorId=0, bool showHidden = false);
        
        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="vendorName">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        IPagedList<Vendor> GetAllVendors(string vendorName,
            int pageIndex, int pageSize,int customerVendorId=0, bool showHidden = false);

        /// <summary>
        /// Gets a vendor
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        Vendor GetVendorById(int vendorId);

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void InsertVendor(Vendor vendor);

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        void UpdateVendor(Vendor vendor);

        /// <summary>
        /// Deletes a product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        void DeleteProductVendor(ProductVendor productVendor);
        
        /// <summary>
        /// Gets product vendor collection
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor collection</returns>
        IPagedList<ProductVendor> GetProductVendorsByVendorId(int vendorId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a product vendor mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor mapping collection</returns>
        IList<ProductVendor> GetProductVendorsByProductId(int productId, bool showHidden = false);

        /// <summary>
        /// Gets a  vendor id 
        /// </summary>
        /// <param name="productId">product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor id</returns>
        int GetVendorIdByProductId(int productId, bool showHidden = false);

        /// <summary>
        /// Get a total number of featured products by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Number of featured products</returns>
        int GetTotalNumberOfFeaturedProducts(int vendorId);

        /// <summary>
        /// Gets a product vendor mapping 
        /// </summary>
        /// <param name="productVendorId">Product vendor mapping identifier</param>
        /// <returns>Product vendor mapping</returns>
        ProductVendor GetProductVendorById(int productVendorId);

        /// <summary>
        /// Inserts a product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        void InsertProductVendor(ProductVendor productVendor);

        /// <summary>
        /// Updates the product vendor mapping
        /// </summary>
        /// <param name="productVendor">Product vendor mapping</param>
        void UpdateProductVendor(ProductVendor productVendor);

        //customer
        /// <summary>
        /// Gets vendor customers collection
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product vendor collection</returns>
        IPagedList<VendorCustomer> GetVendorCustomersByVendorId(int vendorId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a  vendor customer mapping collection
        /// </summary>
        /// <param name="customerId">customer identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor customer mapping collection</returns>
        IList<VendorCustomer> GetVendorsCustomerByCustomerId(int customerId, bool showHidden = false);

        /// <summary>
        /// Gets a  vendor id 
        /// </summary>
        /// <param name="customerId">customer identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>vendor id</returns>
        int GetVendorIdByCustomerId(int customerId, bool showHidden = false);

        /// <summary>
        /// Gets a customer vendor mapping 
        /// </summary>
        /// <param name="vendorCustomerId">Vendor Customer mapping identifier</param>
        /// <returns>Vendor Customer mapping</returns>
        VendorCustomer GetVendorCustomerById(int vendorCustomerId);

        /// <summary>
        /// Updates the vendor customer mapping
        /// </summary>
        /// <param name="vendorCustomer">Vendor Customer mapping</param>
        void UpdateVendorCustomer(VendorCustomer vendorCustomer);

        /// <summary>
        /// Deletes a vendor customer mapping
        /// </summary>
        /// <param name="vendorCustomer">vendor customer mapping</param>
        void DeleteVendorCustomer(VendorCustomer vendorCustomer);

        /// <summary>
        /// Inserts a customer vendor mapping
        /// </summary>
        /// <param name="vendorCustomer">vendor customer mapping</param>
        void InsertVendorCustomer(VendorCustomer vendorCustomer);

        //checkout

        /// <summary>
        /// Get vendor shoping cart items 
        /// </summary>
        /// <param name="shopingcartItems">Shoping cart items</param>
        /// <returns>Vendor Order</returns>
        IList<VendorOrder> getVendorSoppingCartItems(IList<ShoppingCartItem> shoppingCartItems);


    }
}
