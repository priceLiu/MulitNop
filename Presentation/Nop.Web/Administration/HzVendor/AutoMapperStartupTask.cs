//add by hz full page
using AutoMapper;
using Nop.Admin.Models.Catalog;
using Nop.Admin.Models.Common;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Seo;

namespace Nop.Admin.HzVendor
{
    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            //products
            Mapper.CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.NumberOfAvailableVendors, mo => mo.Ignore());//add by hz
            Mapper.CreateMap<ProductModel, Product>()
                .ForMember(dest => dest.ProductVendors, mo => mo.Ignore());//add by hz
            //add by hz
            //vendor
            Mapper.CreateMap<Vendor, VendorModel>()
                .ForMember(dest => dest.AvailableVendorTemplates, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName(0, true, false)))
                .ForMember(dest => dest.AvailableCustomerRoles, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedCustomerRoleIds, mo => mo.Ignore());
            Mapper.CreateMap<VendorModel, Vendor>()
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore());
            //branch
            Mapper.CreateMap<Branch, BranchModel>();
            Mapper.CreateMap<BranchModel, Branch>()
                .ForMember(dest => dest.CreatedOnUtc, dt => dt.Ignore());
            //end by hz
           
            
        }
        
        public int Order
        {
            get { return 0; }
        }
    }
}