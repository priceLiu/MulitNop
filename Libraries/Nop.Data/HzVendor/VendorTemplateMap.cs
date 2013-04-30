//add by hz full page
using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class VendorTemplateMap : EntityTypeConfiguration<VendorTemplate>
    {
        public VendorTemplateMap()
        {
            this.ToTable("VendorTemplate");
            this.HasKey(p => p.Id);
            this.Property(p => p.Name).IsRequired().HasMaxLength(400);
            this.Property(p => p.ViewPath).IsRequired().HasMaxLength(400);
        }
    }
}