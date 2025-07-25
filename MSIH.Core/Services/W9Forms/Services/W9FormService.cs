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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MSIH.Core.Common.Data;
using MSIH.Core.Services.W9Forms.Interfaces;
using MSIH.Core.Services.W9Forms.Models;
using MSIH.Core.Services.W9Forms.Utilities;
using msih.p4g.Shared.W9FormService.Dtos;
using System.Security.Claims;
using W9FormEntity = MSIH.Core.Services.W9Forms.Models.W9Form;

namespace MSIH.Core.Services.W9Forms.Services
{
    /// <summary>
    /// Implementation of the W9 form service
    /// </summary>
    public class W9FormService : IW9FormService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<W9FormService> _logger;

        public W9FormService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            ILogger<W9FormService> logger)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Get a W9 form by ID
        /// </summary>
        public async Task<W9FormDto> GetByIdAsync(int id)
        {
            try
            {
                var w9Form = await _dbContext.W9Forms.FindAsync(id);
                if (w9Form == null)
                {
                    return null;
                }

                // Check if user has access to this form
                if (!await UserHasAccessToFormAsync(w9Form))
                {
                    _logger.LogWarning("User attempted to access W9 form {FormId} without permission", id);
                    return null;
                }

                return MapToDto(w9Form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting W9 form with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Get a W9 form by user ID
        /// </summary>
        public async Task<W9FormDto> GetByUserIdAsync(int userId)
        {
            try
            {
                // Check if the user is accessing their own form or is an admin
                //if (!IsUserOrAdmin(userId))
                //{
                //    _logger.LogWarning("User attempted to access W9 form for user {UserId} without permission", userId);
                //    return null;
                //}

                var w9Form = await _dbContext.W9Forms
                    .Where(w => w.UserId == userId)
                    .OrderByDescending(w => w.ModifiedOn ?? w.CreatedOn)
                    .FirstOrDefaultAsync();

                if (w9Form == null)
                {
                    return null;
                }

                return MapToDto(w9Form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting W9 form for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get a W9 form by fundraiser ID
        /// </summary>
        public async Task<W9FormDto> GetByFundraiserIdAsync(int fundraiserId)
        {
            try
            {
                var w9Form = await _dbContext.W9Forms
                    .Where(w => w.FundraiserId == fundraiserId)
                    .OrderByDescending(w => w.ModifiedOn ?? w.CreatedOn)
                    .FirstOrDefaultAsync();

                if (w9Form == null)
                {
                    return null;
                }

                //// Check if user has access to this form
                //if (!await UserHasAccessToFormAsync(w9Form))
                //{
                //    _logger.LogWarning("User attempted to access W9 form for fundraiser {FundraiserId} without permission", fundraiserId);
                //    return null;
                //}

                return MapToDto(w9Form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting W9 form for fundraiser {FundraiserId}", fundraiserId);
                throw;
            }
        }

        /// <summary>
        /// Save a W9 form (create or update)
        /// </summary>
        public async Task<W9FormDto> SaveW9FormAsync(W9FormDto w9FormDto)
        {
            try
            {
                // Check if the user is authorized to create/update this form
                //if (!IsUserOrAdmin(w9FormDto.UserId))
                //{
                //    _logger.LogWarning("User attempted to save W9 form for user {UserId} without permission", w9FormDto.UserId);
                //    throw new UnauthorizedAccessException("You are not authorized to create or update this W9 form.");
                //}

                W9FormEntity w9Form;
                bool isNew = w9FormDto.Id == 0;

                if (isNew)
                {
                    // Create new form
                    w9Form = new W9FormEntity();
                    MapToEntity(w9FormDto, w9Form);
                    w9Form.CreatedOn = DateTime.UtcNow;
                    w9Form.CreatedBy = GetCurrentUserName();
                    _dbContext.W9Forms.Add(w9Form);
                }
                else
                {
                    // Update existing form
                    w9Form = await _dbContext.W9Forms.FindAsync(w9FormDto.Id);
                    if (w9Form == null)
                    {
                        throw new KeyNotFoundException($"W9 form with ID {w9FormDto.Id} not found.");
                    }

                    // Make sure it belongs to the user
                    if (w9Form.UserId != w9FormDto.UserId && !IsUserInRole("Admin"))
                    {
                        throw new UnauthorizedAccessException("You are not authorized to update this W9 form.");
                    }

                    MapToEntity(w9FormDto, w9Form);
                    w9Form.ModifiedOn = DateTime.UtcNow;
                    w9Form.ModifiedBy = GetCurrentUserName();
                }

                await _dbContext.SaveChangesAsync();
                // update funderaider ojbect for w9Form
                if (w9Form.FundraiserId.HasValue)
                {
                    var fundraiser = await _dbContext.Fundraisers.FindAsync(w9Form.FundraiserId.Value);
                    if (fundraiser != null)
                    {
                        fundraiser.W9Document = "true";
                        _dbContext.Fundraisers.Update(fundraiser);
                        await _dbContext.SaveChangesAsync();
                    }
                }
                return MapToDto(w9Form);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving W9 form for user {UserId}", w9FormDto.UserId);
                throw;
            }
        }

        /// <summary>
        /// Delete a W9 form
        /// </summary>
        public async Task<bool> DeleteW9FormAsync(int id)
        {
            try
            {
                // Only admins can delete forms
                if (!IsUserInRole("Admin"))
                {
                    _logger.LogWarning("Non-admin user attempted to delete W9 form {FormId}", id);
                    throw new UnauthorizedAccessException("Only administrators can delete W9 forms.");
                }

                var w9Form = await _dbContext.W9Forms.FindAsync(id);
                if (w9Form == null)
                {
                    return false;
                }

                _dbContext.W9Forms.Remove(w9Form);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting W9 form with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Update the status of a W9 form
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            try
            {
                // Only admins can update status
                if (!IsUserInRole("Admin"))
                {
                    _logger.LogWarning("Non-admin user attempted to update status of W9 form {FormId}", id);
                    throw new UnauthorizedAccessException("Only administrators can update the status of W9 forms.");
                }

                var w9Form = await _dbContext.W9Forms.FindAsync(id);
                if (w9Form == null)
                {
                    return false;
                }

                w9Form.Status = status;
                w9Form.ModifiedOn = DateTime.UtcNow;
                w9Form.ModifiedBy = GetCurrentUserName();
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of W9 form with ID {Id}", id);
                throw;
            }
        }

        #region Helper Methods

        private bool IsUserOrAdmin(int userId)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int currentUserId))
            {
                return false;
            }

            return currentUserId == userId || IsUserInRole("Admin");
        }

        private bool IsUserInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
        }

        private async Task<bool> UserHasAccessToFormAsync(W9FormEntity form)
        {
            // Users can view their own forms
            if (IsUserOrAdmin(form.UserId))
            {
                return true;
            }

            // TODO: Add additional checks if needed, e.g., for fundraiser managers
            // This would check if the current user manages the fundraiser associated with this form

            return false;
        }

        private string GetCurrentUserName()
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
            return emailClaim ?? "System";
        }

        private W9FormDto MapToDto(W9FormEntity entity)
        {
            return new W9FormDto
            {
                Id = entity.Id,
                Name = entity.Name,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                BusinessName = entity.BusinessName,
                FederalTaxClassification = entity.FederalTaxClassification,
                LLCTaxClassification = entity.LLCTaxClassification,
                OtherClassificationInstructions = entity.OtherClassificationInstructions,
                PartnershipTrustInfo = entity.PartnershipTrustInfo,
                ExemptPayeeCode = entity.ExemptPayeeCode,
                FATCAExemptionCode = entity.FATCAExemptionCode,
                Address = entity.Address,
                CityStateZip = entity.CityStateZip,
                AccountNumbers = entity.AccountNumbers,
                SocialSecurityNumber = SsnUtility.FormatSsnForDisplay(entity.SocialSecurityNumber),
                EmployerIdentificationNumber = SsnUtility.FormatEinForDisplay(entity.EmployerIdentificationNumber),
                SignedDate = entity.SignedDate,
                SignatureVerification = entity.SignatureVerification,
                FundraiserId = entity.FundraiserId,
                UserId = entity.UserId,
                Status = entity.Status,
                CreatedOn = entity.CreatedOn,
                CreatedBy = entity.CreatedBy,
                ModifiedOn = entity.ModifiedOn,
                ModifiedBy = entity.ModifiedBy
            };
        }

        private void MapToEntity(W9FormDto dto, W9FormEntity entity)
        {
            entity.Name = dto.Name;
            entity.FirstName = dto.FirstName;
            entity.LastName = dto.LastName;
            entity.BusinessName = dto.BusinessName;
            entity.FederalTaxClassification = dto.FederalTaxClassification;
            entity.LLCTaxClassification = dto.LLCTaxClassification;
            entity.OtherClassificationInstructions = dto.OtherClassificationInstructions;
            entity.PartnershipTrustInfo = dto.PartnershipTrustInfo;
            entity.ExemptPayeeCode = dto.ExemptPayeeCode;
            entity.FATCAExemptionCode = dto.FATCAExemptionCode;
            entity.Address = dto.Address;
            entity.CityStateZip = dto.CityStateZip;
            entity.AccountNumbers = dto.AccountNumbers;

            // Only update SSN or EIN if they have changed
            if (!string.IsNullOrEmpty(dto.SocialSecurityNumber) &&
                dto.SocialSecurityNumber != SsnUtility.FormatSsnForDisplay(entity.SocialSecurityNumber))
            {
                entity.SocialSecurityNumber = SsnUtility.EncryptEin(dto.SocialSecurityNumber);
                entity.EmployerIdentificationNumber = null; // Clear EIN if SSN is provided
            }

            if (!string.IsNullOrEmpty(dto.EmployerIdentificationNumber) &&
                dto.EmployerIdentificationNumber != SsnUtility.FormatEinForDisplay(entity.EmployerIdentificationNumber))
            {
                entity.EmployerIdentificationNumber = SsnUtility.EncryptEin(dto.EmployerIdentificationNumber);
                entity.SocialSecurityNumber = null; // Clear SSN if EIN is provided
            }

            entity.SignedDate = dto.SignedDate;
            entity.SignatureVerification = dto.SignatureVerification;
            entity.FundraiserId = dto.FundraiserId;
            entity.UserId = dto.UserId;
            entity.Status = dto.Status;
        }

        #endregion
    }
}
