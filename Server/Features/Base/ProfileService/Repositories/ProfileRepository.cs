// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Data;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Features.Base.ProfileService.Repositories
{
    public class ProfileRepository : GenericRepository<Profile, ProfileDbContext>, IProfileRepository
    {
        public ProfileRepository(ProfileDbContext context) : base(context)
        {
        }
        // Add custom methods for Profile if needed
    }
}
