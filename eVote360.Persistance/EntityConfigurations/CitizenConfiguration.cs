
using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Persistance.EntityConfigurations
{
    public class CitizenConfiguration : IEntityTypeConfiguration<Citizen>
    {
        public void Configure(EntityTypeBuilder<Citizen> builder)
        {
            #region Basic configuration
            builder.ToTable("Citizens");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(120);
            builder.Property(x => x.IdentityDocument).IsRequired().HasMaxLength(20);
            builder.HasIndex(x => x.IdentityDocument).IsUnique();
            #endregion

            #region Relationships
            builder.HasMany(x => x.Votes)
                   .WithOne(v => v.Citizen)
                   .HasForeignKey(v => v.CitizenId)
                   .OnDelete(DeleteBehavior.Restrict);
            #endregion
        }
    }
}
