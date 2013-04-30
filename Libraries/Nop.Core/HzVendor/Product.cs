//add by hz full page
using System;
using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>

    public partial class Product : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported
    {
        private ICollection<ProductVendor> _productVendors; //add by hz
        //add by hz
        /// <summary>
        /// Gets or sets the collection of ProductVendor
        /// </summary>
        public virtual ICollection<ProductVendor> ProductVendors
        {
            get { return _productVendors ?? (_productVendors = new List<ProductVendor>()); }
            protected set { _productVendors = value; }
        }
        //end by hz
    }
}