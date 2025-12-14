
using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Persistance.EntityConfigurations
{
    public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
    {
        public void Configure(EntityTypeBuilder<Candidate> builder)
        {
            #region Basic configuration
            builder.ToTable("Candidates");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.PhotoUrl).HasMaxLength(255);
            #endregion

            #region Relationships
            builder.HasOne(x => x.Citizen)
                   .WithMany()
                   .HasForeignKey(x => x.CitizenId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Party)
                   .WithMany(p => p.Candidates)
                   .HasForeignKey(x => x.PartyId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
