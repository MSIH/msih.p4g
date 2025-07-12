// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Model;

namespace msih.p4g.Client.Features.FundraiserManagement.Component
{
    /// <summary>
    /// Component for displaying affiliate commission earnings by quarter
    /// </summary>
    public partial class AffiliateComissions
    {
        [Parameter] public int FundraiserId { get; set; }
        [Parameter] public Profile? Profile { get; set; }

        [Inject] private IFundraiserStatisticsService FundraiserStatsService { get; set; } = default!;
        [Inject] private IPayoutService PayoutService { get; set; } = default!;

        private List<QuarterlyCommission> quarterlyCommissions = new();
        private bool isLoading = true;
        private string errorMessage = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadCommissionDataAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (FundraiserId > 0)
            {
                await LoadCommissionDataAsync();
            }
        }

        private async Task LoadCommissionDataAsync()
        {
            isLoading = true;
            errorMessage = string.Empty;

            try
            {
                if (FundraiserId <= 0)
                {
                    errorMessage = "Invalid fundraiser ID.";
                    return;
                }

                // Get fundraiser statistics (donations)
                var statistics = await FundraiserStatsService.GetReferralDonorsAsync(FundraiserId);

                // Get payouts for this fundraiser
                var payouts = await PayoutService.GetPayoutsByFundraiserIdAsync(FundraiserId.ToString());

                // Calculate quarterly commissions
                // quarterlyCommissions = CalculateQuarterlyCommissions(statistics, payouts);

                // Calculate quarterly affiliate commissions
                quarterlyCommissions = CalculateQuarterlyAffiliateCommissions(statistics, payouts);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading commission data: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }


        private List<QuarterlyCommission> CalculateQuarterlyAffiliateCommissions(List<FirstTimeDonorInfo> statistics, List<Payout> payouts)
        {
            var commissions = new List<QuarterlyCommission>();

            if (statistics == null || !statistics.Any())
            {
                return commissions;
            }

            // Determine the starting date (fundraiser profile creation date or first donation)
            var startDate = Profile?.CreatedOn ?? DateTime.Now;
            var endDate = DateTime.Now;

            // Generate quarters from start date to current date
            var currentQuarter = new DateTime(startDate.Year, ((startDate.Month - 1) / 3) * 3 + 1, 1);
            var cumulativeNewDonors = 0; // Track total new donors across all quarters
            var totalCommissionPayouts = 0m;
            var previousCommissionEarned = 0m; // Track previous quarter's total commission

            while (currentQuarter <= endDate)
            {
                var quarterEnd = currentQuarter.AddMonths(3).AddDays(-1);
                var quarter = (currentQuarter.Month - 1) / 3 + 1;

                // Count new donors who made their first donation in this quarter
                var newDonorsThisQuarter = statistics
                    .Count(d => d.FirstDonationDate >= currentQuarter && d.FirstDonationDate <= quarterEnd);

                // Update cumulative count of new donors
                cumulativeNewDonors += newDonorsThisQuarter;

                // Calculate total commission earned based on cumulative new donors
                var currentTotalCommission = CalculateCommissionForDonorCount(cumulativeNewDonors);

                // Calculate commission earned just this quarter (difference from previous total)
                var quarterlyCommissionEarned = currentTotalCommission - previousCommissionEarned;

                // Get payouts for this quarter
                var quarterPayouts = payouts
                    .Where(p => p.CreatedAt >= currentQuarter && p.CreatedAt <= quarterEnd)
                    .ToList();

                var payoutAmount = quarterPayouts.Sum(p => p.Amount);
                var payoutDate = quarterPayouts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt;
                var payoutStatus = quarterPayouts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.TransactionStatus.ToString();

                totalCommissionPayouts += payoutAmount;

                commissions.Add(new QuarterlyCommission
                {
                    Year = currentQuarter.Year,
                    Quarter = quarter,
                    StartDate = currentQuarter,
                    EndDate = quarterEnd,
                    QuarterlyDonations = 0, // Removed as this is no longer in use
                    NewDonorsThisQuarter = newDonorsThisQuarter, // New property for new donors this quarter
                    TotalNewDonors = cumulativeNewDonors, // New property for cumulative new donors
                    TotalAnnualDonations = 0, // Not used in new structure, but keeping for compatibility
                    CommissionEarned = currentTotalCommission, // Total commission earned up to this quarter
                    QuarterlyCommissionEarned = quarterlyCommissionEarned, // Commission earned just this quarter
                    PayoutAmount = payoutAmount,
                    PayoutDate = payoutDate,
                    PayoutStatus = payoutStatus ?? "NONE",
                    TotalCommissionPayouts = totalCommissionPayouts
                });

                // Update previous commission for next iteration
                previousCommissionEarned = currentTotalCommission;
                currentQuarter = currentQuarter.AddMonths(3);
            }

            return commissions;
        }

        private List<QuarterlyCommission> CalculateQuarterlyCommissions(FundraiserStatistics statistics, List<Payout> payouts)
        {
            var commissions = new List<QuarterlyCommission>();

            if (statistics?.Donations == null || !statistics.Donations.Any())
            {
                return commissions;
            }

            // Get the fundraiser's referral code from the Profile
            var fundraiserReferralCode = Profile?.ReferralCode;
            if (string.IsNullOrEmpty(fundraiserReferralCode))
            {
                return commissions; // Cannot calculate commissions without referral code
            }

            // Filter donations to only include those made after fundraiser account creation
            var validDonations = statistics.Donations
                .Where(d => Profile?.CreatedOn == null || d.DonationDate >= Profile.CreatedOn)
                .ToList();

            if (!validDonations.Any())
            {
                return commissions;
            }

            // Group donations by DonorId to find first donation for each unique donor
            // Handle cases where DonorId might be 0 or invalid by also grouping by DonorName as fallback
            var uniqueDonorFirstDonations = validDonations
                .Where(d => d.DonorId > 0 || !string.IsNullOrEmpty(d.DonorName)) // Filter out invalid entries
                .GroupBy(d => d.DonorId > 0 ? d.DonorId.ToString() : d.DonorName) // Use DonorId if valid, otherwise DonorName
                .Select(group => new
                {
                    DonorKey = group.Key,
                    DonorId = group.First().DonorId,
                    DonorName = group.First().DonorName,
                    FirstDonation = group.OrderBy(d => d.DonationDate).First(), // Get first donation from this donor
                    FirstDonationDate = group.Min(d => d.DonationDate)
                })
                .OrderBy(x => x.FirstDonationDate) // Order by when they first donated
                .ToList();

            if (!uniqueDonorFirstDonations.Any())
            {
                return commissions;
            }

            // Determine the starting date (fundraiser profile creation date or first donation)
            var startDate = Profile?.CreatedOn ?? uniqueDonorFirstDonations.Min(d => d.FirstDonationDate);
            var endDate = DateTime.Now;

            // Generate quarters from start date to current date
            var currentQuarter = new DateTime(startDate.Year, ((startDate.Month - 1) / 3) * 3 + 1, 1);
            var cumulativeNewDonors = 0; // Track total new donors across all quarters
            var totalCommissionPayouts = 0m;
            var previousCommissionEarned = 0m; // Track previous quarter's total commission

            while (currentQuarter <= endDate)
            {
                var quarterEnd = currentQuarter.AddMonths(3).AddDays(-1);
                var quarter = (currentQuarter.Month - 1) / 3 + 1;

                // Count new donors who made their first donation in this quarter
                var newDonorsThisQuarter = uniqueDonorFirstDonations
                    .Count(d => d.FirstDonationDate >= currentQuarter && d.FirstDonationDate <= quarterEnd);

                // Update cumulative count of new donors
                cumulativeNewDonors += newDonorsThisQuarter;

                // Calculate total commission earned based on cumulative new donors
                var currentTotalCommission = CalculateCommissionForDonorCount(cumulativeNewDonors);

                // Calculate commission earned just this quarter (difference from previous total)
                var quarterlyCommissionEarned = currentTotalCommission - previousCommissionEarned;

                // Get donation amounts for this quarter (for display purposes)
                var quarterDonations = validDonations
                    .Where(d => d.DonationDate >= currentQuarter && d.DonationDate <= quarterEnd)
                    .Sum(d => d.Amount);

                // Get payouts for this quarter
                var quarterPayouts = payouts
                    .Where(p => p.CreatedAt >= currentQuarter && p.CreatedAt <= quarterEnd)
                    .ToList();

                var payoutAmount = quarterPayouts.Sum(p => p.Amount);
                var payoutDate = quarterPayouts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.CreatedAt;
                var payoutStatus = quarterPayouts.OrderByDescending(p => p.CreatedAt).FirstOrDefault()?.TransactionStatus.ToString();

                totalCommissionPayouts += payoutAmount;

                commissions.Add(new QuarterlyCommission
                {
                    Year = currentQuarter.Year,
                    Quarter = quarter,
                    StartDate = currentQuarter,
                    EndDate = quarterEnd,
                    QuarterlyDonations = quarterDonations, // Total donations this quarter for reference
                    NewDonorsThisQuarter = newDonorsThisQuarter, // New property for new donors this quarter
                    TotalNewDonors = cumulativeNewDonors, // New property for cumulative new donors
                    TotalAnnualDonations = 0, // Not used in new structure, but keeping for compatibility
                    CommissionEarned = currentTotalCommission, // Total commission earned up to this quarter
                    QuarterlyCommissionEarned = quarterlyCommissionEarned, // Commission earned just this quarter
                    PayoutAmount = payoutAmount,
                    PayoutDate = payoutDate,
                    PayoutStatus = payoutStatus ?? "NONE",
                    TotalCommissionPayouts = totalCommissionPayouts
                });

                // Update previous commission for next iteration
                previousCommissionEarned = currentTotalCommission;
                currentQuarter = currentQuarter.AddMonths(3);
            }

            return commissions;
        }

        /// <summary>
        /// Calculate commission based on the total number of new donors
        /// New structure: 3 donor = $10,  5 donors = $35, then +$35 for every 5 additional donors
        /// </summary>
        private decimal CalculateCommissionForDonorCount(int donorCount)
        {
            if (donorCount <= 0) return 0;

            // 3-4 donors still = $10 (no change until 5)
            if (donorCount >= 3 && donorCount < 5) return 10;

            // 5 donors = $35
            if (donorCount == 5) return 35;

            // For more than 5 donors: $35 base + $35 for every additional group of 5
            if (donorCount > 5)
            {
                var additionalDonors = donorCount - 5;
                var additionalGroups = additionalDonors / 5; // Integer division
                return 35 + (additionalGroups * 35);
            }

            return 0;
        }

        // Keep the old method for backwards compatibility but mark as obsolete
        [Obsolete("This method is replaced by CalculateCommissionForDonorCount for the new donor-based commission structure")]
        private decimal CalculateCommissionForAmount(decimal amount)
        {
            if (amount <= 0) return 0;

            decimal commission = 0;
            decimal remaining = amount;

            // $25 threshold - $5 commission
            if (remaining >= 25)
            {
                commission += 5;
                remaining -= 25;
            }
            else
            {
                return 0; // No commission if under $25
            }

            // $75 more for $100 total - $10 commission
            if (remaining >= 75)
            {
                commission += 10;
                remaining -= 75;
            }
            else
            {
                return commission;
            }

            // $150 more for $250 total - $20 commission
            if (remaining >= 150)
            {
                commission += 20;
                remaining -= 150;
            }
            else
            {
                return commission;
            }

            // Every additional $250 - $35 commission
            while (remaining >= 250)
            {
                commission += 35;
                remaining -= 250;
            }

            return commission;
        }

        private DateTime GetNextPayoutDate(int year, int quarter)
        {
            // Payouts are on the 7th of the month following each quarter
            var payoutMonth = quarter * 3 + 1; // Q1->April, Q2->July, Q3->October, Q4->January
            var payoutYear = payoutMonth > 12 ? year + 1 : year;
            payoutMonth = payoutMonth > 12 ? 1 : payoutMonth;

            return new DateTime(payoutYear, payoutMonth, 7);
        }
    }

    public class QuarterlyCommission
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal QuarterlyDonations { get; set; }

        // New properties for donor-based commission structure
        public int NewDonorsThisQuarter { get; set; } // New donors added this quarter
        public int TotalNewDonors { get; set; } // Cumulative new donors up to this quarter

        public decimal TotalAnnualDonations { get; set; } // Kept for compatibility
        public decimal CommissionEarned { get; set; }
        public decimal QuarterlyCommissionEarned { get; set; } // Commission earned just this quarter
        public decimal PayoutAmount { get; set; }
        public DateTime? PayoutDate { get; set; }
        public string PayoutStatus { get; set; } = string.Empty;
        public decimal TotalCommissionPayouts { get; set; }
    }

    public class DonationInfo
    {
        public int Id { get; set; }
        public string DonorName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string Message { get; set; }

        // Add the missing property
        public int DonorId { get; set; } // Ensure this matches the expected type in the grouping logic
    }
}
