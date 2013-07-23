//add by hz - full page
using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;


namespace Nop.Services.Catalog
{
    public partial class VendorOrder
    {
        public VendorOrder()
        {
            this.vendor = new Vendor();
            this.processPaymentResult = new ProcessPaymentResult();
            this.processPaymentRequest = new ProcessPaymentRequest();
        }
        public Vendor vendor { get; set; }


        public PlaceOrderResult placeOrderResult { get; set; }

        public ProcessPaymentResult processPaymentResult { get; set; }

        public ProcessPaymentRequest processPaymentRequest { get; set; }
        /// <summary>
        /// Gets or sets a shopping cart items
        /// </summary>
        private ICollection<ShoppingCartItem> _ShoppingCartItems;

        /// <summary>
        /// Gets or sets shopping cart items
        /// </summary>
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems
        {
            get { return _ShoppingCartItems ?? (_ShoppingCartItems = new List<ShoppingCartItem>()); }
            protected set { _ShoppingCartItems = value; }
        }
    }
}
