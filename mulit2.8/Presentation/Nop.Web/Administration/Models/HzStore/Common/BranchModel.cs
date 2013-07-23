//add by hz - full page
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Localization;
using Nop.Admin.Validators.Common;


namespace Nop.Admin.Models.Common
{
    [Validator(typeof(BranchValidator))]
    public partial class BranchModel : BaseNopEntityModel, ILocalizedModel<BranchLocalizedModel>
    {
        public BranchModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            Locales = new List<BranchLocalizedModel>();
        }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("Admin.Catalog.Vendors.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Website")]
        [AllowHtml]
        public string Website { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Email")]
        [AllowHtml]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Country")]
        public int? CountryId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Country")]
        [AllowHtml]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.StateProvince")]
        [AllowHtml]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Address2")]
        [AllowHtml]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.ZipPostalCode")]
        [AllowHtml]
        public string ZipPostalCode { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.PhoneNumber")]
        [AllowHtml]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.FaxNumber")]
        [AllowHtml]
        public string FaxNumber { get; set; }

        public IList<BranchLocalizedModel> Locales { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

    }
    public partial class BranchLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.City")]
        [AllowHtml]
        public string City { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Address1")]
        [AllowHtml]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Vendors.Branches.Fields.Address2")]
        [AllowHtml]
        public string Address2 { get; set; }

    }
}