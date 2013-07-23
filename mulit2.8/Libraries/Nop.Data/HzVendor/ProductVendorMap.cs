//add by hz full page
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductVendorMap : EntityTypeConfiguration<ProductVendor>
    {
        public ProductVendorMap()
        {
            this.ToTable("Product_Vendor_Mapping");
            this.HasKey(ps => ps.Id);
            
            this.HasRequired(ps => ps.Vendor)
                .WithMany()
                .HasForeignKey(ps => ps.VendorId);


            this.HasRequired(ps => ps.Product)
                .WithMany(p => p.ProductVendors)
                .HasForeignKey(ps => ps.ProductId);
        }
    }
}