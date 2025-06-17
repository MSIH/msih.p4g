// Utility functions for the application

// Get the window width
function getWindowWidth() {
    return window.innerWidth;
}

// Set up a DotNet reference for the NavMenu component
let navMenuReference = null;

// Function to set the reference
function setNavMenuReference(reference) {
    navMenuReference = reference;
}

// Window resize handler
window.addEventListener('resize', function() {
    if (navMenuReference) {
        navMenuReference.invokeMethodAsync('OnBrowserResize');
    }
});

// Initialize Bootstrap components after the page loads
document.addEventListener('DOMContentLoaded', function() {
    // Initialize all dropdowns
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });
});

// Manual method to show/hide dropdowns via JS
function toggleDropdown(elementId) {
    const dropdown = document.getElementById(elementId);
    if (dropdown) {
        const instance = bootstrap.Dropdown.getInstance(dropdown);
        if (instance) {
            instance.toggle();
        } else {
            new bootstrap.Dropdown(dropdown).toggle();
        }
    }
}
