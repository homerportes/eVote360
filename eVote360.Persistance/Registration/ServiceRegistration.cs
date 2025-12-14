using eVote360.Domain.Interfaces;
using eVote360.Persistence.Contexts;
using eVote360.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eVote360.Persistance.Registration
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            var connectionString = config.GetConnectionString("DefaultConnection");

            services.AddDbContext<EVote360AppContext>(opt =>
                opt.UseSqlServer(connectionString,
                    m => m.MigrationsAssembly(typeof(EVote360AppContext).Assembly.FullName)),
                ServiceLifetime.Transient);
            #endregion

            #region Repositories IOC
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IElectivePositionRepository, ElectivePositionRepository>();
            services.AddTransient<IPoliticalPartyRepository, PoliticalPartyRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ICitizenRepository, CitizenRepository>();
            services.AddTransient<ICandidateRepository, CandidateRepository>();
            services.AddTransient<ICandidacyRepository, CandidacyRepository>();
            services.AddTransient<IElectionRepository, ElectionRepository>();
            services.AddTransient<IElectionCandidacyRepository, ElectionCandidacyRepository>();
            services.AddTransient<IPoliticalAllianceRepository, PoliticalAllianceRepository>();
            services.AddTransient<IVoteRepository, VoteRepository>();
            #endregion

        }
    }
}
