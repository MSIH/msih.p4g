using System.ComponentModel.DataAnnotations;
using msih.p4g.Server.Features.Base.ProfileService.Model;

namespace msih.p4g.Server.Features.DonationService.Models
{
    /// <summary>
    /// DTO for processing a donation request from the client.
    /// </summary>
    public class DonationRequestDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public AddressModel Address { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public bool PayTransactionFee { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsAnnual { get; set; }
        public string? DonationMessage { get; set; }
        public string? ReferralCode { get; set; }
        public string? CampaignCode { get; set; }
        [Required]
        public string PaymentToken { get; set; }
    }
}
