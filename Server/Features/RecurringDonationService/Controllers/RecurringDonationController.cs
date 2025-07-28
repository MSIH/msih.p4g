/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.AspNetCore.Mvc;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.RecurringDonationService.Interfaces;
using msih.p4g.Server.Features.RecurringDonationService.Models;
using msih.p4g.Shared.Models;

namespace msih.p4g.Server.Features.RecurringDonationService.Controllers
{
    /// <summary>
    /// Controller for managing recurring donations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringDonationController : ControllerBase
    {
        private readonly IRecurringDonationService _recurringDonationService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<RecurringDonationController> _logger;

        public RecurringDonationController(
            IRecurringDonationService recurringDonationService,
            IUserRepository userRepository,
            ILogger<RecurringDonationController> logger)
        {
            _recurringDonationService = recurringDonationService;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new recurring donation.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RecurringDonationResponseDto>> CreateRecurringDonation(
            [FromBody] CreateRecurringDonationDto dto,
            [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email is required");
                }

                // Get user and donor
                var user = await _userRepository.GetByEmailAsync(userEmail, includeDonor: true);
                if (user?.Donor == null)
                {
                    return BadRequest("User not found or is not a donor");
                }

                // Create recurring donation
                var recurringDonation = new RecurringDonation
                {
                    DonorId = user.Donor.Id,
                    Amount = dto.Amount,
                    Frequency = dto.Frequency,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    PaymentMethodToken = dto.PaymentMethodToken,
                    PayTransactionFee = dto.PayTransactionFee,
                    PayTransactionFeeAmount = dto.PayTransactionFeeAmount,
                    DonationMessage = dto.DonationMessage,
                    ReferralCode = dto.ReferralCode,
                    CampaignCode = dto.CampaignCode,
                    CampaignId = dto.CampaignId
                };

                var result = await _recurringDonationService.CreateRecurringDonationAsync(recurringDonation, userEmail);

                var response = MapToResponseDto(result);
                return CreatedAtAction(nameof(GetRecurringDonation), new { id = result.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring donation for user {UserEmail}", userEmail);
                return StatusCode(500, "An error occurred while creating the recurring donation");
            }
        }

        /// <summary>
        /// Gets a recurring donation by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RecurringDonationResponseDto>> GetRecurringDonation(int id)
        {
            try
            {
                var recurringDonation = await _recurringDonationService.GetByIdAsync(id);
                if (recurringDonation == null)
                {
                    return NotFound();
                }

                var response = MapToResponseDto(recurringDonation);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring donation {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the recurring donation");
            }
        }

        /// <summary>
        /// Gets recurring donations for a user.
        /// </summary>
        [HttpGet("user/{email}")]
        public async Task<ActionResult<PagedResult<RecurringDonationResponseDto>>> GetUserRecurringDonations(
            string email,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var parameters = new PaginationParameters { PageNumber = pageNumber, PageSize = pageSize };
                var result = await _recurringDonationService.GetPagedByUserEmailAsync(email, parameters);

                var response = new PagedResult<RecurringDonationResponseDto>
                {
                    Items = result.Items.Select(MapToResponseDto).ToList(),
                    TotalCount = result.TotalCount,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recurring donations for user {Email}", email);
                return StatusCode(500, "An error occurred while retrieving recurring donations");
            }
        }

        /// <summary>
        /// Updates a recurring donation.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RecurringDonationResponseDto>> UpdateRecurringDonation(
            int id,
            [FromBody] UpdateRecurringDonationDto dto,
            [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email is required");
                }

                var recurringDonation = await _recurringDonationService.GetByIdAsync(id);
                if (recurringDonation == null)
                {
                    return NotFound();
                }

                // Verify ownership
                if (recurringDonation.Donor?.User?.Email != userEmail)
                {
                    return Forbid("You can only update your own recurring donations");
                }

                // Update fields if provided
                if (dto.Amount.HasValue)
                {
                    await _recurringDonationService.UpdateAmountAsync(id, dto.Amount.Value, userEmail);
                }

                if (!string.IsNullOrEmpty(dto.PaymentMethodToken))
                {
                    await _recurringDonationService.UpdatePaymentMethodAsync(id, dto.PaymentMethodToken, userEmail);
                }

                if (dto.PayTransactionFee.HasValue)
                {
                    recurringDonation.PayTransactionFee = dto.PayTransactionFee.Value;
                }

                if (dto.PayTransactionFeeAmount.HasValue)
                {
                    recurringDonation.PayTransactionFeeAmount = dto.PayTransactionFeeAmount.Value;
                }

                if (dto.DonationMessage != null)
                {
                    recurringDonation.DonationMessage = dto.DonationMessage;
                }

                if (dto.EndDate.HasValue)
                {
                    recurringDonation.EndDate = dto.EndDate;
                }

                await _recurringDonationService.UpdateRecurringDonationAsync(recurringDonation, userEmail);

                // Get updated data
                var updatedRecurringDonation = await _recurringDonationService.GetByIdAsync(id);
                var response = MapToResponseDto(updatedRecurringDonation!);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring donation {Id}", id);
                return StatusCode(500, "An error occurred while updating the recurring donation");
            }
        }

        /// <summary>
        /// Pauses a recurring donation.
        /// </summary>
        [HttpPost("{id}/pause")]
        public async Task<ActionResult> PauseRecurringDonation(int id, [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email is required");
                }

                var recurringDonation = await _recurringDonationService.GetByIdAsync(id);
                if (recurringDonation == null)
                {
                    return NotFound();
                }

                // Verify ownership
                if (recurringDonation.Donor?.User?.Email != userEmail)
                {
                    return Forbid("You can only pause your own recurring donations");
                }

                var result = await _recurringDonationService.PauseRecurringDonationAsync(id, userEmail);
                if (!result)
                {
                    return BadRequest("Unable to pause the recurring donation");
                }

                return Ok(new { message = "Recurring donation paused successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing recurring donation {Id}", id);
                return StatusCode(500, "An error occurred while pausing the recurring donation");
            }
        }

        /// <summary>
        /// Resumes a paused recurring donation.
        /// </summary>
        [HttpPost("{id}/resume")]
        public async Task<ActionResult> ResumeRecurringDonation(int id, [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email is required");
                }

                var recurringDonation = await _recurringDonationService.GetByIdAsync(id);
                if (recurringDonation == null)
                {
                    return NotFound();
                }

                // Verify ownership
                if (recurringDonation.Donor?.User?.Email != userEmail)
                {
                    return Forbid("You can only resume your own recurring donations");
                }

                var result = await _recurringDonationService.ResumeRecurringDonationAsync(id, userEmail);
                if (!result)
                {
                    return BadRequest("Unable to resume the recurring donation");
                }

                return Ok(new { message = "Recurring donation resumed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming recurring donation {Id}", id);
                return StatusCode(500, "An error occurred while resuming the recurring donation");
            }
        }

        /// <summary>
        /// Cancels a recurring donation.
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelRecurringDonation(
            int id,
            [FromBody] CancelRecurringDonationDto dto,
            [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("User email is required");
                }

                var recurringDonation = await _recurringDonationService.GetByIdAsync(id);
                if (recurringDonation == null)
                {
                    return NotFound();
                }

                // Verify ownership
                if (recurringDonation.Donor?.User?.Email != userEmail)
                {
                    return Forbid("You can only cancel your own recurring donations");
                }

                var result = await _recurringDonationService.CancelRecurringDonationAsync(id, userEmail, dto.Reason);
                if (!result)
                {
                    return BadRequest("Unable to cancel the recurring donation");
                }

                return Ok(new { message = "Recurring donation cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring donation {Id}", id);
                return StatusCode(500, "An error occurred while cancelling the recurring donation");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Maps a RecurringDonation entity to a response DTO.
        /// </summary>
        private RecurringDonationResponseDto MapToResponseDto(RecurringDonation recurringDonation)
        {
            return new RecurringDonationResponseDto
            {
                Id = recurringDonation.Id,
                Amount = recurringDonation.Amount,
                Frequency = recurringDonation.Frequency,
                Currency = recurringDonation.Currency,
                Status = recurringDonation.Status,
                StartDate = recurringDonation.StartDate,
                EndDate = recurringDonation.EndDate,
                NextProcessDate = recurringDonation.NextProcessDate,
                LastProcessedDate = recurringDonation.LastProcessedDate,
                SuccessfulDonationsCount = recurringDonation.SuccessfulDonationsCount,
                FailedAttemptsCount = recurringDonation.FailedAttemptsCount,
                PayTransactionFee = recurringDonation.PayTransactionFee,
                PayTransactionFeeAmount = recurringDonation.PayTransactionFeeAmount,
                DonationMessage = recurringDonation.DonationMessage,
                ReferralCode = recurringDonation.ReferralCode,
                CampaignCode = recurringDonation.CampaignCode,
                CampaignId = recurringDonation.CampaignId,
                CampaignName = recurringDonation.Campaign?.Title,
                LastErrorMessage = recurringDonation.LastErrorMessage,
                CancelledDate = recurringDonation.CancelledDate,
                CancelledBy = recurringDonation.CancelledBy,
                CancellationReason = recurringDonation.CancellationReason,
                CreatedOn = recurringDonation.CreatedOn,
                ModifiedOn = recurringDonation.ModifiedOn
            };
        }

        #endregion
    }
}