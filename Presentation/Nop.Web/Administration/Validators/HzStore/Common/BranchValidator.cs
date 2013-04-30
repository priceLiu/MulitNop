using FluentValidation;
using Nop.Admin.Models.Common;
using Nop.Services.Localization;

namespace Nop.Admin.Validators.Common
{
    public class BranchValidator : AbstractValidator<BranchModel>
    {
        public BranchValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name)
                 .NotNull()
                 .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.Name.Required"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"))
                ;
            RuleFor(x => x.CountryId)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.Country.Required"))
                ;
            RuleFor(x => x.CountryId)
                .NotEqual(0)
                .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.Country.Required"))
                ;
            RuleFor(x => x.City)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.City.Required"))
                ;
            RuleFor(x => x.Address1)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.Address1.Required"))
                ;
            RuleFor(x => x.PhoneNumber)
                .NotNull()
                .WithMessage(localizationService.GetResource("Admin.Catalog.Vendors.Branches.Fields.PhoneNumber.Required"))
                ;
        }
    }
}