/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using msih.p4g.Server.Features.OrganizationService.Models;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Shared.OrganizationService.Dtos
{
    /// <summary>
    /// Data transfer object for Organization entity
    /// </summary>
    public class OrganizationDto
    {
        /// <summary>
        /// The organization ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Legal name of the organization
        /// </summary>
        [Required(ErrorMessage = "Legal name is required")]
        [MaxLength(200, ErrorMessage = "Legal name cannot exceed 200 characters")]
        public string LegalName { get; set; }
        
        /// <summary>
        /// Organization's tax ID (EIN for US organizations)
        /// </summary>
        [Required(ErrorMessage = "Tax ID is required")]
        [MaxLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string TaxId { get; set; }
        
        /// <summary>
        /// Organization's website URL
        /// </summary>
        [MaxLength(255, ErrorMessage = "Website URL cannot exceed 255 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Website { get; set; }
        
        /// <summary>
        /// Primary email address for the organization
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [MaxLength(100, ErrorMessage = "Email address cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string EmailAddress { get; set; }
        
        /// <summary>
        /// Primary phone number for the organization
        /// </summary>
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string Phone { get; set; }
        
        /// <summary>
        /// Organization's mission statement
        /// </summary>
        [MaxLength(1000, ErrorMessage = "Mission statement cannot exceed 1000 characters")]
        public string MissionStatement { get; set; }
        
        /// <summary>
        /// Organization's short description
        /// </summary>
        [MaxLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
        public string ShortDescription { get; set; }
        
        /// <summary>
        /// The type of non-profit organization (e.g., 501(c)(3), 501(c)(4), etc.)
        /// </summary>
        [MaxLength(50, ErrorMessage = "Organization type cannot exceed 50 characters")]
        public string OrganizationType { get; set; }
        
        /// <summary>
        /// Logo URL for the organization
        /// </summary>
        [MaxLength(255, ErrorMessage = "Logo URL cannot exceed 255 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string LogoUrl { get; set; }
        
        /// <summary>
        /// Street address of the organization
        /// </summary>
        [MaxLength(200, ErrorMessage = "Street address cannot exceed 200 characters")]
        public string Street { get; set; }
        
        /// <summary>
        /// City of the organization
        /// </summary>
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; }
        
        /// <summary>
        /// State/province of the organization
        /// </summary>
        [MaxLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string State { get; set; }
        
        /// <summary>
        /// Postal/zip code of the organization
        /// </summary>
        [MaxLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; }
        
        /// <summary>
        /// Country of the organization
        /// </summary>
        [MaxLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; }
        
        /// <summary>
        /// Whether the organization is active
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Creates a DTO from an Organization entity
        /// </summary>
        /// <param name="organization">The organization entity</param>
        /// <returns>The organization DTO</returns>
        public static OrganizationDto FromEntity(Organization organization)
        {
            return new OrganizationDto
            {
                Id = organization.Id,
                LegalName = organization.LegalName,
                TaxId = organization.TaxId,
                Website = organization.Website,
                EmailAddress = organization.EmailAddress,
                Phone = organization.Phone,
                MissionStatement = organization.MissionStatement,
                ShortDescription = organization.ShortDescription,
                OrganizationType = organization.OrganizationType,
                LogoUrl = organization.LogoUrl,
                Street = organization.Street,
                City = organization.City,
                State = organization.State,
                PostalCode = organization.PostalCode,
                Country = organization.Country,
                IsActive = organization.IsActive
            };
        }
        
        /// <summary>
        /// Creates an Organization entity from a DTO
        /// </summary>
        /// <returns>The organization entity</returns>
        public Organization ToEntity()
        {
            return new Organization
            {
                Id = Id,
                LegalName = LegalName,
                TaxId = TaxId,
                Website = Website,
                EmailAddress = EmailAddress,
                Phone = Phone,
                MissionStatement = MissionStatement,
                ShortDescription = ShortDescription,
                OrganizationType = OrganizationType,
                LogoUrl = LogoUrl,
                Street = Street,
                City = City,
                State = State,
                PostalCode = PostalCode,
                Country = Country,
                IsActive = IsActive
            };
        }
    }
}
