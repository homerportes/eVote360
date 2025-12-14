
using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Persistance.EntityConfigurations
{
    public class PoliticalAllianceConfiguration : IEntityTypeConfiguration<PoliticalAlliance>
    {
        public void Configure(EntityTypeBuilder<PoliticalAlliance> builder)
        {
            #region Basic configuration
            builder.ToTable("PoliticalAlliances");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.RequestedAt).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            #endregion

            #region Relationships
            builder.HasOne(x => x.RequestingParty)
                   .WithMany(p => p.RequestedAlliances)
                   .HasForeignKey(x => x.RequestingPartyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReceivingParty)
                   .WithMany(p => p.ReceivedAlliances)
                   .HasForeignKey(x => x.ReceivingPartyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.RequestingPartyId, x.ReceivingPartyId })
                   .IsUnique();
            #endregion
        }
    }
}
