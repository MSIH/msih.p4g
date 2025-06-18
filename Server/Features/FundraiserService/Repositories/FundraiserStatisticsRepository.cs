/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Server.Features.FundraiserService.Repositories
{
    /// <summary>
    /// Repository implementation for fundraiser statistics
    /// </summary>
    public class FundraiserStatisticsRepository : IFundraiserStatisticsRepository
    {
        private readonly ApplicationDbContext _dbContext;
        
        /// <summary>
        /// Initializes a new instance of the FundraiserStatisticsRepository class
        /// </summary>
        /// <param name="dbContext">The application database context</param>
        public FundraiserStatisticsRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        /// <inheritdoc />
        public async Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId)
        {
            // Get the fundraiser to retrieve the user ID
            var fundraiser = await _dbContext.Fundraisers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == fundraiserId);

            if (fundraiser == null)
            {
                return new FundraiserStatistics();
            }

            // Get the profile to get the referral code
            var profile = await _dbContext.Profiles
                .FirstOrDefaultAsync(p => p.UserId == fundraiser.UserId);
                
            if (profile == null || string.IsNullOrEmpty(profile.ReferralCode))
            {
                return new FundraiserStatistics();
            }

            // Get all donations that used this fundraiser's referral code
            var donations = await _dbContext.Donations
                .Include(d => d.Donor)
                .ThenInclude(d => d.User)
                .ThenInclude(u => u.Profile)
                .Where(d => d.ReferralCode == profile.ReferralCode)
                .OrderByDescending(d => d.CreatedOn)
                .ToListAsync();

            // Calculate statistics
            var statistics = new FundraiserStatistics
            {
                DonationCount = donations.Count,
                TotalRaised = donations.Sum(d => d.Amount),
                AverageDonation = donations.Any() ? donations.Average(d => d.Amount) : 0,
                Donations = donations.Select(d => new DonationInfo
                {
                    Id = d.Id,
                    DonorName = $"{d.Donor.User.Profile.FirstName} {d.Donor.User.Profile.LastName}",
                    Amount = d.Amount,
                    DonationDate = d.CreatedOn,
                    Message = d.DonationMessage ?? string.Empty
                }).ToList()
            };

            return statistics;
        }
        
        /// <inheritdoc />
        public async Task<string?> GetReferralCodeAsync(int userId)
        {
            var profile = await _dbContext.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);
                
            return profile?.ReferralCode;
        }
    }
}
