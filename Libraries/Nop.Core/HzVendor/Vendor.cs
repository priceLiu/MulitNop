//add by hz full page
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a vendor
    /// </summary>
    public partial class Vendor : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported 
    {
        private ICollection<Branch> _branches;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets a value of used vendor template identifier
        /// </summary>
        public virtual int VendorTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public virtual string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public virtual string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public virtual string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent picture identifier
        /// </summary>
        public virtual int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public virtual int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
        public virtual bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
        public virtual string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
        public virtual string PriceRanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public virtual bool SubjectToAcl { get; set; } 

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public virtual bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public virtual bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public virtual DateTime UpdatedOnUtc { get; set; }


        private ICollection<VendorCustomer> _vendorCustomers; //add by hz
        //add by hz
        /// <summary>
        /// Gets or sets the collection of VendorCustomer
        /// </summary>
        public virtual ICollection<VendorCustomer> VendorCustomers
        {
            get { return _vendorCustomers ?? (_vendorCustomers = new List<VendorCustomer>()); }
            protected set { _vendorCustomers = value; }
        }
        //end by hz

        /// <summary>
        /// Gets or sets vendor branches
        /// </summary>
        public virtual ICollection<Branch> Branches
        {
            get { return _branches ?? (_branches = new List<Branch>()); }
            protected set { _branches = value; }
        }

        #region branches

        public virtual void AddBranch(Branch branch)
        {
            if (!this.Branches.Contains(branch))
                this.Branches.Add(branch);
        }

        public virtual void RemoveBranch(Branch branch)
        {
            if (this.Branches.Contains(branch))
                this.Branches.Remove(branch);
        }

        #endregion
    }
}
