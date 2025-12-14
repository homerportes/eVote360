using eVote360.Application.Interfaces;
using eVote360.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace eVote360.Application.Registration
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region Services IOC
            services.AddTransient<IElectivePositionService, ElectivePositionService>();
            services.AddTransient<IPoliticalPartyService, PoliticalPartyService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ICitizenService, CitizenService>();
            services.AddTransient<ICandidateService, CandidateService>();
            services.AddTransient<ICandidacyService, CandidacyService>();
            services.AddTransient<IElectionService, ElectionService>();
            services.AddTransient<IPoliticalAllianceService, PoliticalAllianceService>();

            // Voting services
            services.AddTransient<IVoteService, VoteService>();
            services.AddTransient<IVotingValidator, VotingValidatorService>();
            services.AddTransient<IIdentityValidator, IdentityValidatorService>();
            #endregion
        }

    }
}
