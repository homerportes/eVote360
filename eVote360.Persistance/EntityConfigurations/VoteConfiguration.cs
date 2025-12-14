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
    public class VoteConfiguration : IEntityTypeConfiguration<Vote>
    {
        public void Configure(EntityTypeBuilder<Vote> builder)
        {
            #region Basic configuration
            builder.ToTable("Votes");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.VoteDate).IsRequired();
            #endregion

            #region Relationships
            builder.HasOne(x => x.Citizen)
                   .WithMany(c => c.Votes)
                   .HasForeignKey(x => x.CitizenId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ElectivePosition)
                   .WithMany(e => e.Votes)
                   .HasForeignKey(x => x.ElectivePositionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Candidate)
                   .WithMany(c => c.Votes)
                   .HasForeignKey(x => x.CandidateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CitizenId, x.ElectionId, x.ElectivePositionId })
                   .IsUnique();
            #endregion
        }
    }
}
