using Microsoft.AspNetCore.Mvc;
using msih.p4g.Server.Features.Base.SmsService.Interfaces;
using msih.p4g.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SmsService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhoneValidationController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IValidatedPhoneNumberRepository _phoneNumberRepository;

        public PhoneValidationController(
            ISmsService smsService,
            IValidatedPhoneNumberRepository phoneNumberRepository)
        {
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _phoneNumberRepository = phoneNumberRepository ?? throw new ArgumentNullException(nameof(phoneNumberRepository));
        }

        /// <summary>
        /// Validates a phone number
        /// </summary>
        /// <param name="phoneNumber">The phone number to validate in E.164 format</param>
        /// <param name="useCache">Whether to use cached validation results if available</param>
        /// <param name="usePaidService">Whether to use the paid carrier lookup service</param>
        /// <returns>The validated phone number information</returns>
        [HttpGet("validate")]
        public async Task<ActionResult<ValidatedPhoneNumber>> ValidatePhoneNumber(
            [FromQuery] string phoneNumber,
            [FromQuery] bool useCache = true,
            [FromQuery] bool usePaidService = false)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return BadRequest("Phone number is required");
            }

            try
            {
                var result = await _smsService.ValidatePhoneNumberAsync(phoneNumber, useCache, usePaidService);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error validating phone number: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all validated phone numbers
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive numbers</param>
        /// <param name="includeDeleted">Whether to include deleted numbers</param>
        /// <returns>A list of validated phone numbers</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ValidatedPhoneNumber>>> GetAll(
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool includeDeleted = false)
        {
            try
            {
                // This uses the generic repository methods through the inheritance chain
                var repository = _phoneNumberRepository as Server.Common.Data.Repositories.IGenericRepository<ValidatedPhoneNumber>;
                
                if (repository != null)
                {
                    var result = await repository.GetAllAsync(includeInactive, includeDeleted);
                    return Ok(result);
                }
                else
                {
                    // Fallback if the repository doesn't implement IGenericRepository
                    return StatusCode(500, "Repository does not implement IGenericRepository");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving phone numbers: {ex.Message}");
            }
        }
    }
}
