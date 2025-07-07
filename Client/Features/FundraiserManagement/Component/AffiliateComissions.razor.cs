using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.FundraiserService.Model;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Models;
using msih.p4g.Server.Features.Base.PayoutService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Model;

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
                var statistics = await FundraiserStatsService.GetStatisticsAsync(FundraiserId);
                
                // Get payouts for this fundraiser
                var payouts = await PayoutService.GetPayoutsByFundraiserIdAsync(FundraiserId.ToString());

                // Calculate quarterly commissions
                quarterlyCommissions = CalculateQuarterlyCommissions(statistics, payouts);
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

        private List<QuarterlyCommission> CalculateQuarterlyCommissions(FundraiserStatistics statistics, List<Payout> payouts)
        {
            var commissions = new List<QuarterlyCommission>();
            
            if (statistics?.Donations == null || !statistics.Donations.Any())
            {
                return commissions;
            }

            // Determine the starting date (fundraiser profile creation date or first donation)
            var startDate = Profile?.CreatedOn ?? statistics.Donations.Min(d => d.DonationDate);
            var endDate = DateTime.Now;

            // Generate quarters from start date to current date
            var currentQuarter = new DateTime(startDate.Year, ((startDate.Month - 1) / 3) * 3 + 1, 1);
            var runningAnnualTotal = 0m;
            var totalCommissionPayouts = 0m;
            var previousAnnualCommission = 0m; // Track previous quarter's annual commission

            while (currentQuarter <= endDate)
            {
                var quarterEnd = currentQuarter.AddMonths(3).AddDays(-1);
                var quarter = (currentQuarter.Month - 1) / 3 + 1;

                // Get donations for this quarter
                var quarterDonations = statistics.Donations
                    .Where(d => d.DonationDate >= currentQuarter && d.DonationDate <= quarterEnd)
                    .Sum(d => d.Amount);

                // Update running annual total
                runningAnnualTotal += quarterDonations;

                // Calculate annual commission based on total annual donations up to this quarter
                var currentAnnualCommission = CalculateCommissionForAmount(runningAnnualTotal);
                
                // Calculate quarterly commission (difference from previous quarter's annual commission)
                var quarterlyCommissionEarned = currentAnnualCommission - previousAnnualCommission;

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
                    QuarterlyDonations = quarterDonations,
                    TotalAnnualDonations = runningAnnualTotal,
                    CommissionEarned = currentAnnualCommission, // Total annual commission up to this quarter
                    QuarterlyCommissionEarned = quarterlyCommissionEarned, // Commission earned just this quarter
                    PayoutAmount = payoutAmount,
                    PayoutDate = payoutDate,
                    PayoutStatus = payoutStatus ?? "NONE",
                    TotalCommissionPayouts = totalCommissionPayouts
                });

                // Update previous annual commission for next iteration
                previousAnnualCommission = currentAnnualCommission;
                currentQuarter = currentQuarter.AddMonths(3);
            }

            return commissions;
        }

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
        public decimal TotalAnnualDonations { get; set; }
        public decimal CommissionEarned { get; set; }
        public decimal QuarterlyCommissionEarned { get; set; } // New property for quarterly commission
        public decimal PayoutAmount { get; set; }
        public DateTime? PayoutDate { get; set; }
        public string PayoutStatus { get; set; } = string.Empty;
        public decimal TotalCommissionPayouts { get; set; }
    }
}
