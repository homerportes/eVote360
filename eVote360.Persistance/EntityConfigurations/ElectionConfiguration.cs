
using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eVote360.Persistance.EntityConfigurations
{
    public class ElectionConfiguration : IEntityTypeConfiguration<Election>
    {
        public void Configure(EntityTypeBuilder<Election> builder)
        {
            #region Basic configuration
            builder.ToTable("Elections");
            builder.HasKey(x => x.Id);
            #endregion

            #region Property configurations
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Status).IsRequired();

            builder.Property(x => x.Date)
                   .HasConversion(
                       v => v.ToDateTime(TimeOnly.MinValue),
                       v => DateOnly.FromDateTime(v)
                   )
                   .IsRequired();
            #endregion

            #region Relationships
            builder.HasMany(x => x.Votes)
                   .WithOne(v => v.Election)
                   .HasForeignKey(v => v.ElectionId)
                   .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
