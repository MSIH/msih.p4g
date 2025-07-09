// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
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
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        /// <summary>
        /// Initializes a new instance of the FundraiserStatisticsRepository class
        /// </summary>
        /// <param name="contextFactory">The application database context factory</param>
        public FundraiserStatisticsRepository(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <inheritdoc />
        public async Task<FundraiserStatistics> GetStatisticsAsync(int fundraiserId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Get the fundraiser to retrieve the user ID
            var fundraiser = await context.Fundraisers
                .Include(f => f.User)
                .FirstOrDefaultAsync(f => f.Id == fundraiserId);

            if (fundraiser == null)
            {
                return new FundraiserStatistics();
            }

            // Get the profile to get the referral code
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == fundraiser.UserId);

            if (profile == null || string.IsNullOrEmpty(profile.ReferralCode))
            {
                return new FundraiserStatistics();
            }

            // Get all donations that used this fundraiser's referral code
            var donations = await context.Donations
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
                TotalRaised = donations.Sum(d => d.DonationAmount),
                AverageDonation = donations.Any() ? donations.Average(d => d.DonationAmount) : 0,
                Donations = donations.Select(d => new DonationInfo
                {
                    Id = d.Id,
                    DonorName = $"{d.Donor.User.Profile.FirstName} {d.Donor.User.Profile.LastName}",
                    Amount = d.DonationAmount,
                    DonationDate = d.CreatedOn,
                    Message = d.DonationMessage ?? string.Empty,
                    DonorId = d.DonorId // Add the missing DonorId
                }).ToList()
            };

            return statistics;
        }

        /// <inheritdoc />
        public async Task<string?> GetReferralCodeAsync(int userId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return profile?.ReferralCode;
        }

        public async Task<List<FirstTimeDonorInfo>> GetReferralDonorsAsync(int fundraiserId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Get the donors from referral code
            var fundraiser = await context.Fundraisers
                .Include(f => f.User)
                .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(f => f.Id == fundraiserId);

            if (fundraiser == null || fundraiser.User?.Profile == null)
            {
                return new List<FirstTimeDonorInfo>();
            }

            var fundraiserReferralCode = fundraiser.User.Profile.ReferralCode;

            //// Get all donors with the fundraiser's referral code who have not made any donations and have confirmed their email
            //var donorsFromReferralJustLogIn = await context.Donors
            //    .Where(d => d.ReferralCode == fundraiserReferralCode)
            //    .Where(d => !d.Donations.Any())
            //    .Where(d => d.User.EmailConfirmedAt != null)
            //    .ToListAsync();

            //// Get all donors with the fundraiser's referral code who have made donations
            //var donorsFromReferral = await context.Donors
            //    .Where(d => d.ReferralCode == fundraiserReferralCode)
            //    .Where(d => d.Donations.Any())
            //    .ToListAsync();

            //// Combine the two lists
            //donorsFromReferral.AddRange(donorsFromReferralJustLogIn);

            var donorsFromReferral = await context.Donors
                .Where(d => d.ReferralCode == fundraiserReferralCode)
                .Where(d => d.User.EmailConfirmedAt != null || d.Donations.Any())
                .ToListAsync();


            return donorsFromReferral.Select(ftd => new FirstTimeDonorInfo
            {
                DonorId = 0,
                DonorName = "",
                FirstDonationDate = ftd.CreatedOn,
                FirstDonationAmount = ftd.UserId
            }).OrderBy(x => x.FirstDonationDate).ToList();
        }


        /// <inheritdoc />
        public async Task<List<FirstTimeDonorInfo>> GetFirstTimeDonorsAsync(int fundraiserId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            // Get the fundraiser and their referral code
            var fundraiser = await context.Fundraisers
                .Include(f => f.User)
                .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(f => f.Id == fundraiserId);

            if (fundraiser == null || fundraiser.User?.Profile == null)
            {
                return new List<FirstTimeDonorInfo>();
            }

            var profile = fundraiser.User.Profile;
            var fundraiserReferralCode = profile.ReferralCode;
            var fundraiserCreationDate = profile.CreatedOn;

            if (string.IsNullOrEmpty(fundraiserReferralCode))
            {
                return new List<FirstTimeDonorInfo>();
            }

            // Get all donors who made their very first donation with this fundraiser
            var firstTimeDonors = await context.Donations
                .Include(d => d.Donor)
                .ThenInclude(d => d.User)
                .ThenInclude(u => u.Profile)
                .Where(d => d.CreatedOn >= fundraiserCreationDate) // Only donations after fundraiser creation
                .GroupBy(d => d.DonorId)
                .Select(g => new
                {
                    DonorId = g.Key,
                    FirstDonation = g.OrderBy(d => d.CreatedOn).First(),
                    DonorFirstDonationEver = context.Donations
                        .Where(dd => dd.DonorId == g.Key)
                        .OrderBy(dd => dd.CreatedOn)
                        .First()
                })
                .Where(x => x.FirstDonation.Id == x.DonorFirstDonationEver.Id && // Their first donation ever
                           x.FirstDonation.ReferralCode == fundraiserReferralCode) // Was with this fundraiser
                .ToListAsync();

            return firstTimeDonors.Select(ftd => new FirstTimeDonorInfo
            {
                DonorId = ftd.DonorId,
                DonorName = $"{ftd.FirstDonation.Donor.User.Profile.FirstName} {ftd.FirstDonation.Donor.User.Profile.LastName}",
                FirstDonationDate = ftd.FirstDonation.CreatedOn,
                FirstDonationAmount = ftd.FirstDonation.DonationAmount
            }).OrderBy(x => x.FirstDonationDate).ToList();
        }
    }
}
