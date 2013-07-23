using System.Data.Entity.ModelConfiguration;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    public partial class BranchMap : EntityTypeConfiguration<Branch>
    {
        public BranchMap()
        {
            this.ToTable("Branch");
            this.HasKey(b => b.Id);

            this.HasOptional(b => b.Country)
                .WithMany()
                .HasForeignKey(b => b.CountryId).WillCascadeOnDelete(false);

            this.HasOptional(b => b.StateProvince)
                .WithMany()
                .HasForeignKey(b => b.StateProvinceId).WillCascadeOnDelete(false);
        }
    }
}
