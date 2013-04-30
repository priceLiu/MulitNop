using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Common
{
    public partial class BranchModel : BaseNopEntityModel
    {
        public BranchModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? StateProvinceId { get; set; }
        public string StateProvinceName { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }

    }
}