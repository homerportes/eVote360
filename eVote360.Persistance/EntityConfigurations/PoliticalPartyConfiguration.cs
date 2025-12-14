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
    public class PoliticalPartyConfiguration : IEntityTypeConfiguration<PoliticalParty>
    {
        public void Configure(EntityTypeBuilder<PoliticalParty> builder)
        {
            #region Basic configuration
            builder.ToTable("PoliticalParties");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.Name).IsRequired().HasMaxLength(120);
            builder.Property(x => x.Acronym).IsRequired().HasMaxLength(15);
            builder.HasIndex(x => x.Acronym).IsUnique();
            builder.Property(x => x.Description).HasMaxLength(250);
            builder.Property(x => x.LogoUrl).HasMaxLength(255);
            #endregion
        }
    }
}
