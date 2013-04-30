//add by hz full page
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Catalog
{
    public partial class VendorMap : EntityTypeConfiguration<Vendor>
    {
        public VendorMap()
        {
            this.ToTable("Vendor");
            this.HasKey(m => m.Id);
            this.Property(m => m.Name).IsRequired().HasMaxLength(400);
            this.Property(m => m.Description);
            this.Property(m => m.MetaKeywords).HasMaxLength(400);
            this.Property(m => m.MetaDescription);
            this.Property(m => m.MetaTitle).HasMaxLength(400);
            this.Property(m => m.PriceRanges).HasMaxLength(400);
            this.Property(m => m.PageSizeOptions).HasMaxLength(200);

            this.HasMany<Branch>(s => s.Branches)
                .WithMany()
                .Map(m => m.ToTable("VendorBranches"));
        }
    }
}