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
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Models;

namespace msih.p4g.Server.Features.OrganizationService.Services
{
    /// <summary>
    /// Service implementation for Organization operations
    /// </summary>
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;

        /// <summary>
        /// Initializes a new instance of the OrganizationService class
        /// </summary>
        /// <param name="organizationRepository">The organization repository</param>
        public OrganizationService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Organization>> GetAllAsync(bool includeInactive = false)
        {
            return await _organizationRepository.GetAllAsync(includeInactive);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Organization>> GetAllWithRelatedDataAsync(bool includeInactive = false)
        {
            return await _organizationRepository.GetAllWithRelatedDataAsync(includeInactive);
        }

        /// <inheritdoc />
        public async Task<Organization?> GetByIdAsync(int id, bool includeInactive = false)
        {
            return await _organizationRepository.GetByIdAsync(id, includeInactive);
        }

        /// <inheritdoc />
        public async Task<Organization?> GetByTaxIdAsync(string taxId, bool includeInactive = false)
        {
            return await _organizationRepository.GetByTaxIdAsync(taxId, includeInactive);
        }

        /// <inheritdoc />
        public async Task<Organization?> GetWithRelatedDataAsync(int id, bool includeInactive = false)
        {
            return await _organizationRepository.GetWithRelatedDataAsync(id, includeInactive);
        }

        /// <inheritdoc />
        public async Task<Organization> AddAsync(Organization organization, string createdBy = "OrganizationService")
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            // Validate that an organization with the same tax ID doesn't already exist
            var existingOrg = await _organizationRepository.GetByTaxIdAsync(organization.TaxId, includeInactive: true);
            if (existingOrg != null)
            {
                throw new InvalidOperationException($"An organization with tax ID {organization.TaxId} already exists.");
            }

            return await _organizationRepository.AddAsync(organization, createdBy);
        }

        /// <inheritdoc />
        public async Task<Organization> UpdateAsync(Organization organization, string modifiedBy = "OrganizationService")
        {
            if (organization == null)
            {
                throw new ArgumentNullException(nameof(organization));
            }

            // If tax ID is changed, ensure no other organization has the same tax ID
            var existingOrg = await _organizationRepository.GetByIdAsync(organization.Id);
            if (existingOrg != null && existingOrg.TaxId != organization.TaxId)
            {
                var orgWithSameTaxId = await _organizationRepository.GetByTaxIdAsync(organization.TaxId, includeInactive: true);
                if (orgWithSameTaxId != null && orgWithSameTaxId.Id != organization.Id)
                {
                    throw new InvalidOperationException($"Another organization with tax ID {organization.TaxId} already exists.");
                }
            }

            return await _organizationRepository.UpdateAsync(organization, modifiedBy);
        }

        /// <inheritdoc />
        public async Task<bool> SetActiveStatusAsync(int id, bool isActive, string modifiedBy = "OrganizationService")
        {
            return await _organizationRepository.SetActiveStatusAsync(id, isActive, modifiedBy);
        }
    }
}
