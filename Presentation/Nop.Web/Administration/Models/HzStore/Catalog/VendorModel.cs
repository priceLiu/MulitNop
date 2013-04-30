//add by hz full page
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Admin.Models.Customers;
using Nop.Admin.Validators.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;

namespace Nop.Admin.Models.Catalog
{
    [Validator(typeof(VendorValidator))]
    public partial class VendorModel : BaseNopEntityModel, ILocalizedModel<VendorLocalizedModel>
    {
        public VendorModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<VendorLocalizedModel>();
            AvailableVendorTemplates = new List<SelectListItem>();
        }

        public bool IsVendorManager { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.VendorTemplate")]
        [AllowHtml]
        public int VendorTemplateId { get; set; }
        public IList<SelectListItem> AvailableVendorTemplates { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [UIHint("Picture")]
        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Picture")]
        public int PictureId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.PageSize")]
        public int PageSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.PriceRanges")]
        [AllowHtml]
        public string PriceRanges { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Deleted")]
        public bool Deleted { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        public IList<VendorLocalizedModel> Locales { get; set; }

        //ACL
	    [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.SubjectToAcl")]
	    public bool SubjectToAcl { get; set; }
	    [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.AclCustomerRoles")]
	    public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
	    public int[] SelectedCustomerRoleIds { get; set; }

        #region Nested classes

        public partial class VendorCustomerModel : BaseNopEntityModel
        {
            public int VendorId { get; set; }

            public int CustomerId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Vendors.Customers.Fields.Customer")]
            public string CustomerName { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Vendors.Customers.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }
        }

        public partial class VendorProductModel : BaseNopEntityModel
        {
            public int VendorId { get; set; }

            public int ProductId { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Vendors.Products.Fields.Product")]
            public string ProductName { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Vendors.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Vendors.Products.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }
        }

        public partial class AddVendorProductModel : BaseNopModel
        {
            public AddVendorProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableVendors = new List<SelectListItem>();
            }
            public GridModel<ProductModel> Products { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [NopResourceDisplayName("Admin.Catalog.Products.List.SearchVendor")]
            public int SearchVendorId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableVendors { get; set; }

            public int VendorId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

        public partial class AddVendorCustomerModel : BaseNopModel
        {
            public GridModel<CustomerModel> Customers { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
            [AllowHtml]
            public string SearchEmail { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.List.SearchUsername")]
            [AllowHtml]
            public string SearchUsername { get; set; }
            public bool UsernamesEnabled { get; set; }

            [NopResourceDisplayName("Admin.Customers.Customers.List.SearchFirstName")]
            [AllowHtml]
            public string SearchFirstName { get; set; }
            [NopResourceDisplayName("Admin.Customers.Customers.List.SearchLastName")]
            [AllowHtml]
            public string SearchLastName { get; set; }

            public int VendorId { get; set; }

            public int[] SelectedCustomerIds { get; set; }
        }
        #endregion
    }

    public partial class VendorLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}