/**
 * Braintree payment integration script
 */
// Global references
let braintreeClient = null;
let dotNetReference = null;
let isPaymentReady = false;

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
    applePay: {
      displayName: 'My Store',
      paymentRequest: {
        total: {
          label: 'My Store',
          amount: Number(parseFloat(document.getElementById('amount')?.value || '0').toFixed(2))
        }
      }
    },
  }, (err, instance) => {
    if (err) {
      console.error('Error creating Drop-in:', err);
      return;
    }
    
    // Assign the instance to global reference
    braintreeClient = instance;
    
    // Listen for payment method change events
    instance.on('paymentMethodRequestable', (event) => {
      // The payment method is ready, enable the submit button
      isPaymentReady = true;
      dotNetReference.invokeMethodAsync('SetPaymentFormReady', true);
      // DON'T call requestPaymentMethod here - this was causing the UI to close!
    });
    
    instance.on('noPaymentMethodRequestable', () => {
      // No payment method is ready, disable the submit button
      isPaymentReady = false;
      dotNetReference.invokeMethodAsync('SetPaymentFormReady', false);
    });
  });
}

/**
 * Get payment method nonce when form is actually submitted
 * @returns {Promise} Promise that resolves with payment data
 */
window.getBraintreeNonce = function() {
  return new Promise((resolve, reject) => {
    if (!braintreeClient || !isPaymentReady) {
      reject('Payment method not ready');
      return;
    }
    
    braintreeClient.requestPaymentMethod((err, payload) => {
      if (err) {
        reject(err);
        return;
      }
      
      // Pass the payment method data back
      resolve({
        nonce: payload.nonce,
        deviceData: payload.deviceData,
        type: payload.type
      });
    });
  });
};

/**
 * Reset the Braintree Drop-in UI
 */
window.resetBraintree = function () {
  if (braintreeClient) {
    braintreeClient.clearSelectedPaymentMethod();
    isPaymentReady = false;
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
