using Nop.Admin.Models.Common;
using Nop.Web.Framework.Mvc;

namespace Nop.Admin.Models.Catalog
{
    public partial class VendorBranchModel : BaseNopModel
    {
        public int VendorId { get; set; }

        public BranchModel Branch { get; set; }
    }
}