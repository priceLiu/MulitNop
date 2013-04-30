//add by hz full page
//add by hz isHzVendor method
//ExportProductsToXml , ExportProductsToXlsx
using System.Collections.Generic;
using System.IO;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager interface
    /// </summary>
    public partial interface IExportManager
    {
        //add by hz
        /// <summary>
        /// Export vendor list to xml
        /// </summary>
        /// <param name="vendors">Vendors</param>
        /// <returns>Result in XML format</returns>
        string ExportVendorsToXml(IList<Vendor> vendors);
        //end by hz

        /// <summary>
        /// Export product list to xml
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>Result in XML format</returns>
        string ExportProductsToXml(IList<Product> products, bool IsHzVendor);

        /// <summary>
        /// Export products to XLSX
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        void ExportProductsToXlsx(Stream stream, IList<Product> products, bool IsHzVendor);

    }
}
