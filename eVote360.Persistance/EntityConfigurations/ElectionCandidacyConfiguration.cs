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
    public class ElectionCandidacyConfiguration : IEntityTypeConfiguration<ElectionCandidacy>
    {
        public void Configure(EntityTypeBuilder<ElectionCandidacy> builder)
        {
            #region Basic configuration
            builder.ToTable("ElectionCandidacies");
            builder.HasKey(x => x.Id);
            #endregion

            #region Relationships
            builder.HasOne(x => x.Election)
                   .WithMany(e => e.ElectionCandidacies)
                   .HasForeignKey(x => x.ElectionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Candidacy)
                   .WithMany(c => c.ElectionCandidacies)
                   .HasForeignKey(x => x.CandidacyId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.ElectionId, x.CandidacyId })
                   .IsUnique();
            #endregion
        }
    }
}
