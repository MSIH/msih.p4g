/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using Microsoft.EntityFrameworkCore;

namespace msih.p4g.Server.Features.Base.ProfileService.Repositories
{
    public class ProfileRepository : GenericRepository<Profile>, IProfileRepository
    {
        public ProfileRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }
        // Add custom methods for Profile if needed
    }
}
