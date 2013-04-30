//add by hz full page
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class VendorListModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Catalog.Vendors.List.SearchVendorName")]
        [AllowHtml]
        public string SearchVendorName { get; set; }

        public bool IsVendorManager { get; set; }
        
        public GridModel<VendorModel> Vendors { get; set; }
    }
}