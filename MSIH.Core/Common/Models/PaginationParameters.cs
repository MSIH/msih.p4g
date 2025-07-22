/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

namespace MSIH.Core.Common.Models
{
    /// <summary>
    /// Pagination parameters for data requests
    /// </summary>
    public class PaginationParameters
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value; 
        }

        public string? SearchTerm { get; set; }
        public string? FilterType { get; set; }
    }
}