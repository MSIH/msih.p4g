/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Shared.W9FormService.Dtos;

namespace msih.p4g.Server.Features.Base.W9FormService.Interfaces
{
    /// <summary>
    /// Interface for W9 form management
    /// </summary>
    public interface IW9FormService
    {
        /// <summary>
        /// Get a W9 form by ID
        /// </summary>
        Task<W9FormDto> GetByIdAsync(int id);
        
        /// <summary>
        /// Get a W9 form by user ID
        /// </summary>
        Task<W9FormDto> GetByUserIdAsync(int userId);
        
        /// <summary>
        /// Get a W9 form by fundraiser ID
        /// </summary>
        Task<W9FormDto> GetByFundraiserIdAsync(int fundraiserId);
        
        /// <summary>
        /// Save a W9 form (create or update)
        /// </summary>
        Task<W9FormDto> SaveW9FormAsync(W9FormDto w9FormDto);
        
        /// <summary>
        /// Delete a W9 form
        /// </summary>
        Task<bool> DeleteW9FormAsync(int id);
        
        /// <summary>
        /// Update the status of a W9 form
        /// </summary>
        Task<bool> UpdateStatusAsync(int id, string status);
    }
}
