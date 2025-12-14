using eVote360.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace eVote360.Persistence.Contexts
{
    public class EVote360AppContext : DbContext
    {
        public EVote360AppContext(DbContextOptions<EVote360AppContext> options) : base(options) { }

        #region DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Citizen> Citizens { get; set; }
        public DbSet<PoliticalParty> PoliticalParties { get; set; }
        public DbSet<PoliticalAlliance> PoliticalAlliances { get; set; }
        public DbSet<ElectivePosition> ElectivePositions { get; set; }
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Candidacy> Candidacies { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<ElectionCandidacy> ElectionCandidacies { get; set; }
        public DbSet<Vote> Votes { get; set; }
        #endregion

        #region OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        #endregion
    }
}

