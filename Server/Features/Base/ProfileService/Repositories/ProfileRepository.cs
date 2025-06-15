using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Campaign.Data;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.Base.ProfileService.Repositories
{
    public class ProfileRepository : GenericRepository<Profile, CampaignDbContext>, IProfileRepository
    {
        public ProfileRepository(CampaignDbContext context) : base(context)
        {
        }
        // Add custom methods for Profile if needed
    }
}
