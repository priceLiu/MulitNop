//add by hz full page
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Vendor template service interface
    /// </summary>
    public partial interface IVendorTemplateService
    {
        /// <summary>
        /// Delete vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        void DeleteVendorTemplate(VendorTemplate vendorTemplate);

        /// <summary>
        /// Gets all vendor templates
        /// </summary>
        /// <returns>Vendor templates</returns>
        IList<VendorTemplate> GetAllVendorTemplates();

        /// <summary>
        /// Gets a vendor template
        /// </summary>
        /// <param name="vendorTemplateId">Vendor template identifier</param>
        /// <returns>Vendor template</returns>
        VendorTemplate GetVendorTemplateById(int vendorTemplateId);

        /// <summary>
        /// Inserts vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        void InsertVendorTemplate(VendorTemplate vendorTemplate);

        /// <summary>
        /// Updates the vendor template
        /// </summary>
        /// <param name="vendorTemplate">Vendor template</param>
        void UpdateVendorTemplate(VendorTemplate vendorTemplate);
    }
}
