using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Persistance.EntityConfigurations
{
    public class ElectivePositionConfiguration : IEntityTypeConfiguration<ElectivePosition>
    {
        public void Configure(EntityTypeBuilder<ElectivePosition> builder)
        {
            #region Basic configuration
            builder.ToTable("ElectivePositions");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(200);
            #endregion
        }
    }
}
