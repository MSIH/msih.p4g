/**
 * Braintree payment integration script
 */

// Global references
let braintreeClient = null;
let dotNetReference = null;

/**
 * Initialize the Braintree Drop-in UI
 * @param {string} clientToken - The client token from Braintree
 * @param {object} dotNetObj - The .NET object reference for callbacks
 */
window.initializeBraintree = function (clientToken, dotNetObj) {
    dotNetReference = dotNetObj;
    
    // Load the Braintree script dynamically if it doesn't exist
    if (!document.getElementById('braintree-script')) {
        const script = document.createElement('script');
        script.id = 'braintree-script';
        script.src = 'https://js.braintreegateway.com/web/dropin/1.33.7/js/dropin.min.js';
        script.async = true;
        script.onload = () => createDropInUI(clientToken);
        document.body.appendChild(script);
    } else {
        createDropInUI(clientToken);
    }
};

/**
 * Create the Braintree Drop-in UI
 * @param {string} clientToken - The client token from Braintree
 */
function createDropInUI(clientToken) {
    const dropinContainer = document.getElementById('dropin-container');
    
    if (!dropinContainer) {
        console.error('Drop-in container not found');
        return;
    }
    
    // Clear any existing content
    dropinContainer.innerHTML = '';
    
    // Create the Drop-in UI
    braintree.dropin.create({
        authorization: clientToken,
        container: '#dropin-container',
        dataCollector: true,
        paypal: {
            flow: 'vault'
        },
        card: {
            cardholderName: {
                required: true
            }
        }
    }, (err, instance) => {
        if (err) {
            console.error('Error creating Drop-in:', err);
            return;
        }
        
        braintreeClient = instance;
        
        // Listen for payment method change events
        instance.on('paymentMethodRequestable', (event) => {
            // The payment method is ready, enable the submit button
            dotNetReference.invokeMethodAsync('SetPaymentFormReady', true);
            
            // Get the payment method nonce
            instance.requestPaymentMethod((err, payload) => {
                if (err) {
                    console.error('Error requesting payment method:', err);
                    return;
                }
                
                // Pass the payment method nonce back to .NET
                dotNetReference.invokeMethodAsync('SetPaymentNonce', payload.nonce, payload.deviceData);
            });
        });
        
        instance.on('noPaymentMethodRequestable', () => {
            // No payment method is ready, disable the submit button
            dotNetReference.invokeMethodAsync('SetPaymentFormReady', false);
        });
    });
}

/**
 * Reset the Braintree Drop-in UI
 */
window.resetBraintree = function () {
    if (braintreeClient) {
        braintreeClient.clearSelectedPaymentMethod();
    }
};

/**
 * Show an alert dialog
 * @param {string} title - The alert title
 * @param {string} message - The alert message
 */
window.showAlert = function (title, message) {
    alert(`${title}\n${message}`);
};
