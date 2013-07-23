//add by hz full page
using System;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Common
{
    public class Branch : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the branch name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the website
        /// </summary>
        public virtual string Website { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public virtual int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the state/province identifier
        /// </summary>
        public virtual int? StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// Gets or sets the address 1
        /// </summary>
        public virtual string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address 2
        /// </summary>
        public virtual string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the zip/postal code
        /// </summary>
        public virtual string ZipPostalCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the fax number
        /// </summary>
        public virtual string FaxNumber { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public virtual Country Country { get; set; }

        /// <summary>
        /// Gets or sets the state/province
        /// </summary>
        public virtual StateProvince StateProvince { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        public object Clone()
        {
            var addr = new Branch()
            {
                Name = this.Name,
                Website = this.Website,
                Email = this.Email,
                Country = this.Country,
                CountryId = this.CountryId,
                StateProvince = this.StateProvince,
                StateProvinceId = this.StateProvinceId,
                City = this.City,
                Address1 = this.Address1,
                Address2 = this.Address2,
                ZipPostalCode = this.ZipPostalCode,
                PhoneNumber = this.PhoneNumber,
                FaxNumber = this.FaxNumber,
                CreatedOnUtc = this.CreatedOnUtc,
                DisplayOrder = this.DisplayOrder,
            };
            return addr;
        }
    }
}
