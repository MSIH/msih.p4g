using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Models;
using System.Threading.Tasks;

namespace msih.p4g.Client.Features.Admin.Components
{
    /// <summary>
    /// Component for displaying organization information
    /// </summary>
    public partial class OrganizationDetail
    {
        [Parameter]
        public int OrganizationId { get; set; }
        
        [Inject]
        private IOrganizationService OrganizationService { get; set; }
        
        [Inject]
        private IJSRuntime JSRuntime { get; set; }
        
        private Organization Organization { get; set; }
        private bool IsLoading { get; set; } = true;
        private string ErrorMessage { get; set; }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            try
            {
                IsLoading = true;
                Organization = await OrganizationService.GetWithRelatedDataAsync(OrganizationId);
                
                if (Organization == null)
                {
                    ErrorMessage = "Organization not found.";
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Error loading organization: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Opens the organization's website in a new tab
        /// </summary>
        private async Task OpenWebsite()
        {
            if (!string.IsNullOrEmpty(Organization?.Website))
            {
                await JSRuntime.InvokeVoidAsync("open", Organization.Website, "_blank");
            }
        }
    }
}
