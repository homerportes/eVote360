using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eVote360.Persistance.EntityConfigurations
{
    public class CandidacyConfiguration : IEntityTypeConfiguration<Candidacy>
    {
        public void Configure(EntityTypeBuilder<Candidacy> builder)
        {
            #region Basic configuration
            builder.ToTable("Candidacies");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.IsAlliance).IsRequired();
            #endregion

            #region Relationships
            builder.HasOne(x => x.Candidate)
                   .WithMany(c => c.Candidacies)
                   .HasForeignKey(x => x.CandidateId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ElectivePosition)
                   .WithMany(e => e.Candidacies)
                   .HasForeignKey(x => x.ElectivePositionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PostulatingParty)
                   .WithMany(p => p.Candidacies)
                   .HasForeignKey(x => x.PostulatingPartyId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
