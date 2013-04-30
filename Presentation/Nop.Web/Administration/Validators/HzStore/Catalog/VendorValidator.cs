//add by hz full page
using FluentValidation;
using Nop.Admin.Models.Catalog;
using Nop.Services.Localization;

namespace Nop.Admin.Validators.Catalog
{
    public class VendorValidator : AbstractValidator<VendorModel>
    {
        public VendorValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotNull().WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Fields.Name.Required"));
        }
    }
}