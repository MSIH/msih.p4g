using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Base.ProfileService.Repositories
{
    public class ProfileRepository : GenericRepository<Profile, msih.p4g.Server.Features.CampaignService.Data.ProfileDbContext>, IProfileRepository
    {
        public ProfileRepository(msih.p4g.Server.Features.CampaignService.Data.ProfileDbContext context) : base(context)
        {
        }
        // Add custom methods for Profile if needed
    }
}
