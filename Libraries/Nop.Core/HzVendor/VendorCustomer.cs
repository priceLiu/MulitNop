//add by hz full page
using Nop.Core.Domain.Customers;
namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product vendor mapping
    /// </summary>
    public partial class VendorCustomer : BaseEntity
    {
        /// <summary>
        /// Gets or sets the sroe identifier
        /// </summary>
        public virtual int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public virtual int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the vendor
        /// </summary>
        public virtual Vendor Vendor { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }
    }

}
