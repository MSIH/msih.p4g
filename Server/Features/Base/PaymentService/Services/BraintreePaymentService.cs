/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Braintree;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Models;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Shared.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using BraintreeEnvironment = Braintree.Environment;

namespace msih.p4g.Server.Features.Base.PaymentService.Services
{
    /// <summary>
    /// Braintree implementation of the payment service
    /// </summary>
    public class BraintreePaymentService : IPaymentService
    {
        private readonly ILogger<BraintreePaymentService> _logger;
        private readonly ISettingsService _settingsService;
        private readonly IPaymentTransactionRepository _transactionRepository;
        
        private BraintreeGateway _gateway = null!;
        private bool _isInitialized = false;
        
        // Cached settings
        private string _merchantId = null!;
        private string _publicKey = null!;
        private string _privateKey = null!;
        private string _environment = null!;
        
        /// <summary>
        /// Gets the name of the payment provider
        /// </summary>
        public string ProviderName => "Braintree";
        
        public BraintreePaymentService(
            ILogger<BraintreePaymentService> logger,
            ISettingsService settingsService,
            IPaymentTransactionRepository transactionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        }
        
        /// <summary>
        /// Initializes the Braintree gateway with configuration from settings
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
                
            try
            {
                // Get settings from the settings service
                _merchantId = await _settingsService.GetValueAsync("Braintree:MerchantId") 
                    ?? throw new Exception("Braintree MerchantId not configured");
                    
                _publicKey = await _settingsService.GetValueAsync("Braintree:PublicKey") 
                    ?? throw new Exception("Braintree PublicKey not configured");
                    
                _privateKey = await _settingsService.GetValueAsync("Braintree:PrivateKey") 
                    ?? throw new Exception("Braintree PrivateKey not configured");
                    
                _environment = await _settingsService.GetValueAsync("Braintree:Environment") ?? "Sandbox";
                
                // Create the gateway based on environment
                if (_environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
                {
                    _gateway = new BraintreeGateway(
                        BraintreeEnvironment.PRODUCTION,
                        _merchantId,
                        _publicKey,
                        _privateKey);
                }
                else
                {
                    _gateway = new BraintreeGateway(
                        BraintreeEnvironment.SANDBOX,
                        _merchantId,
                        _publicKey,
                        _privateKey);
                }
                
                _isInitialized = true;
                _logger.LogInformation("Braintree payment service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Braintree payment service");
                throw;
            }
        }
        
        /// <summary>
        /// Generates a client token for the Braintree client SDK
        /// </summary>
        public async Task<ClientTokenResponse> GenerateClientTokenAsync(Models.ClientTokenRequest request)
        {
            try
            {
                await InitializeAsync();
                
                // Create a Braintree token request
                var braintreeTokenRequest = new Braintree.ClientTokenRequest();
                
                if (!string.IsNullOrEmpty(request.CustomerId))
                {
                    braintreeTokenRequest.CustomerId = request.CustomerId;
                }
                
                var clientToken = await _gateway.ClientToken.GenerateAsync(braintreeTokenRequest);
                
                return new ClientTokenResponse
                {
                    Success = true,
                    ClientToken = clientToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Braintree client token");
                return new ClientTokenResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to generate client token: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// Processes a payment using Braintree
        /// </summary>
        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                await InitializeAsync();
                
                var transactionRequest = new TransactionRequest
                {
                    Amount = request.Amount,
                    PaymentMethodNonce = request.PaymentMethodNonce,
                    OrderId = request.OrderReference,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true,
                        StoreInVaultOnSuccess = false
                    }
                };
                
                if (!string.IsNullOrEmpty(request.DeviceData))
                {
                    transactionRequest.DeviceData = request.DeviceData;
                }
                
                if (!string.IsNullOrEmpty(request.CustomerEmail))
                {
                    transactionRequest.Customer = new CustomerRequest
                    {
                        Email = request.CustomerEmail
                    };
                }
                
                var result = await _gateway.Transaction.SaleAsync(transactionRequest);
                
                if (result.IsSuccess())
                {
                    var transaction = result.Target;
                    
                    // Save the transaction to the database
                    var paymentTransaction = new PaymentTransaction
                    {
                        TransactionId = transaction.Id,
                        Provider = ProviderName,
                        Amount = transaction.Amount.Value,
                        Currency = transaction.CurrencyIsoCode,
                        Status = MapBraintreeStatusToPaymentStatus(transaction.Status),
                        Description = request.Description ?? "",
                        ProcessedOn = DateTime.UtcNow,
                        CustomerEmail = request.CustomerEmail ?? "",
                        OrderReference = request.OrderReference ?? "",
                        AdditionalData = JsonConvert.SerializeObject(new 
                        {
                            CardType = transaction.CreditCard?.CardType.ToString(),
                            LastFour = transaction.CreditCard?.LastFour,
                            ExpirationMonth = transaction.CreditCard?.ExpirationMonth,
                            ExpirationYear = transaction.CreditCard?.ExpirationYear
                        }),
                        CreatedBy = "System"
                    };
                    
                    await _transactionRepository.AddAsync(paymentTransaction);
                    
                    return new PaymentResponse
                    {
                        Success = true,
                        TransactionId = transaction.Id,
                        Status = transaction.Status.ToString(),
                        Amount = transaction.Amount.Value,
                        Currency = transaction.CurrencyIsoCode,
                        CardLastFour = transaction.CreditCard?.LastFour ?? "",
                        CardType = transaction.CreditCard?.CardType.ToString() ?? ""
                    };
                }
                else
                {
                    var errorMessage = result.Message;
                    
                    if (result.Errors.DeepCount > 0)
                    {
                        errorMessage = string.Join(", ", result.Errors.DeepAll().Select(e => e.Message));
                    }
                    
                    // Save the failed transaction to the database
                    var paymentTransaction = new PaymentTransaction
                    {
                        TransactionId = result.Transaction?.Id ?? Guid.NewGuid().ToString(),
                        Provider = ProviderName,
                        Amount = request.Amount,
                        Currency = request.Currency,
                        Status = PaymentStatus.Failed,
                        Description = request.Description ?? "",
                        ProcessedOn = DateTime.UtcNow,
                        CustomerEmail = request.CustomerEmail ?? "",
                        OrderReference = request.OrderReference ?? "",
                        ErrorMessage = errorMessage,
                        CreatedBy = "System"
                    };
                    
                    await _transactionRepository.AddAsync(paymentTransaction);
                    
                    return new PaymentResponse
                    {
                        Success = false,
                        TransactionId = result.Transaction?.Id ?? "",
                        Status = result.Transaction?.Status.ToString() ?? "Failed",
                        ErrorMessage = errorMessage,
                        Amount = request.Amount,
                        Currency = request.Currency
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment with Braintree");
                
                // Save the exception to the database
                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    Provider = ProviderName,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Status = PaymentStatus.Failed,
                    Description = request.Description ?? "",
                    ProcessedOn = DateTime.UtcNow,
                    CustomerEmail = request.CustomerEmail ?? "",
                    OrderReference = request.OrderReference ?? "",
                    ErrorMessage = ex.Message,
                    CreatedBy = "System"
                };
                
                await _transactionRepository.AddAsync(paymentTransaction);
                
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to process payment: {ex.Message}",
                    Amount = request.Amount,
                    Currency = request.Currency
                };
            }
        }
        
        /// <summary>
        /// Processes a refund through Braintree
        /// </summary>
        public async Task<RefundResponse> ProcessRefundAsync(RefundRequest request)
        {
            try
            {
                await InitializeAsync();
                
                Result<Transaction> result;
                
                // If amount is specified, do a partial refund
                if (request.Amount.HasValue)
                {
                    result = await _gateway.Transaction.RefundAsync(request.TransactionId, request.Amount.Value);
                }
                else
                {
                    // Otherwise do a full refund
                    result = await _gateway.Transaction.RefundAsync(request.TransactionId);
                }
                
                if (result.IsSuccess())
                {
                    var refundTransaction = result.Target;
                    
                    // Get the original transaction
                    var originalTransaction = await GetTransactionDetailsAsync(request.TransactionId);
                    
                    if (originalTransaction != null)
                    {
                        // Update the original transaction status
                        var refundedStatus = request.Amount.HasValue && request.Amount.Value < originalTransaction.Amount
                            ? PaymentStatus.PartiallyRefunded
                            : PaymentStatus.Refunded;
                            
                        await _transactionRepository.UpdateStatusAsync(
                            originalTransaction.Id, 
                            refundedStatus, 
                            null, 
                            "System");
                    }
                    
                    // Save the refund transaction
                    var refundRecord = new PaymentTransaction
                    {
                        TransactionId = refundTransaction.Id,
                        Provider = ProviderName,
                        Amount = refundTransaction.Amount.Value,
                        Currency = refundTransaction.CurrencyIsoCode,
                        Status = MapBraintreeStatusToPaymentStatus(refundTransaction.Status),
                        Description = $"Refund for transaction {request.TransactionId}: {request.Reason ?? ""}",
                        ProcessedOn = DateTime.UtcNow,
                        CustomerEmail = originalTransaction?.CustomerEmail ?? "",
                        OrderReference = originalTransaction?.OrderReference ?? "",
                        AdditionalData = JsonConvert.SerializeObject(new 
                        {
                            OriginalTransactionId = request.TransactionId,
                            Reason = request.Reason
                        }),
                        CreatedBy = "System"
                    };
                    
                    await _transactionRepository.AddAsync(refundRecord);
                    
                    return new RefundResponse
                    {
                        Success = true,
                        RefundTransactionId = refundTransaction.Id,
                        OriginalTransactionId = request.TransactionId,
                        Status = refundTransaction.Status.ToString(),
                        Amount = refundTransaction.Amount.Value,
                        Currency = refundTransaction.CurrencyIsoCode
                    };
                }
                else
                {
                    var errorMessage = result.Message;
                    
                    if (result.Errors.DeepCount > 0)
                    {
                        errorMessage = string.Join(", ", result.Errors.DeepAll().Select(e => e.Message));
                    }
                    
                    return new RefundResponse
                    {
                        Success = false,
                        OriginalTransactionId = request.TransactionId,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for transaction {request.TransactionId}");
                return new RefundResponse
                {
                    Success = false,
                    OriginalTransactionId = request.TransactionId,
                    ErrorMessage = $"Failed to process refund: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// Voids a transaction through Braintree
        /// </summary>
        public async Task<bool> VoidTransactionAsync(string transactionId)
        {
            try
            {
                await InitializeAsync();
                
                var result = await _gateway.Transaction.VoidAsync(transactionId);
                
                if (result.IsSuccess())
                {
                    // Update the transaction in our database
                    var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                    
                    if (transaction != null)
                    {
                        await _transactionRepository.UpdateStatusAsync(
                            transaction.Id, 
                            PaymentStatus.Voided, 
                            null, 
                            "System");
                    }
                    
                    return true;
                }
                else
                {
                    var errorMessage = result.Message;
                    
                    if (result.Errors.DeepCount > 0)
                    {
                        errorMessage = string.Join(", ", result.Errors.DeepAll().Select(e => e.Message));
                    }
                    
                    _logger.LogError($"Failed to void transaction {transactionId}: {errorMessage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error voiding transaction {transactionId}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets transaction details from Braintree and our database
        /// </summary>
        public async Task<PaymentTransaction?> GetTransactionDetailsAsync(string transactionId)
        {
            try
            {
                // First check our database
                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                
                if (transaction != null)
                {
                    return transaction;
                }
                
                // If not found, try to get it from Braintree
                await InitializeAsync();
                
                var braintreeTransaction = await _gateway.Transaction.FindAsync(transactionId);
                
                if (braintreeTransaction != null)
                {
                    // Create a record in our database
                    var paymentTransaction = new PaymentTransaction
                    {
                        TransactionId = braintreeTransaction.Id,
                        Provider = ProviderName,
                        Amount = braintreeTransaction.Amount ?? 0,
                        Currency = braintreeTransaction.CurrencyIsoCode ?? "USD",
                        Status = MapBraintreeStatusToPaymentStatus(braintreeTransaction.Status),
                        ProcessedOn = braintreeTransaction.CreatedAt ?? DateTime.UtcNow,
                        AdditionalData = JsonConvert.SerializeObject(new 
                        {
                            CardType = braintreeTransaction.CreditCard?.CardType.ToString(),
                            LastFour = braintreeTransaction.CreditCard?.LastFour,
                            ExpirationMonth = braintreeTransaction.CreditCard?.ExpirationMonth,
                            ExpirationYear = braintreeTransaction.CreditCard?.ExpirationYear
                        }),
                        Description = "",
                        CustomerEmail = "",
                        OrderReference = braintreeTransaction.OrderId ?? "",
                        ErrorMessage = "",
                        CreatedBy = "System"
                    };
                    
                    await _transactionRepository.AddAsync(paymentTransaction);
                    return paymentTransaction;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving transaction details for {transactionId}");
                return null;
            }
        }
        
        /// <summary>
        /// Updates a transaction's status from Braintree
        /// </summary>
        public async Task<PaymentTransaction?> UpdateTransactionStatusAsync(string transactionId)
        {
            try
            {
                await InitializeAsync();
                
                // Get the transaction from our database
                var transaction = await _transactionRepository.GetByTransactionIdAsync(transactionId);
                
                if (transaction == null)
                {
                    _logger.LogWarning($"Transaction {transactionId} not found in database");
                    return null;
                }
                
                // Get the latest status from Braintree
                var braintreeTransaction = await _gateway.Transaction.FindAsync(transactionId);
                
                if (braintreeTransaction != null)
                {
                    var newStatus = MapBraintreeStatusToPaymentStatus(braintreeTransaction.Status);
                    
                    // Update the status if it has changed
                    if (transaction.Status != newStatus)
                    {
                        await _transactionRepository.UpdateStatusAsync(
                            transaction.Id, 
                            newStatus, 
                            null, 
                            "System");
                            
                        transaction.Status = newStatus;
                    }
                    
                    return transaction;
                }
                
                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating transaction status for {transactionId}");
                return null;
            }
        }
        
        /// <summary>
        /// Maps Braintree transaction status to our PaymentStatus enum
        /// </summary>
        private PaymentStatus MapBraintreeStatusToPaymentStatus(TransactionStatus status)
        {
            switch (status)
            {
                case TransactionStatus.AUTHORIZED:
                    return PaymentStatus.Authorized;
                    
                case TransactionStatus.AUTHORIZING:
                    return PaymentStatus.Pending;
                    
                case TransactionStatus.SETTLEMENT_PENDING:
                    return PaymentStatus.Settling;
                    
                case TransactionStatus.SETTLED:
                    return PaymentStatus.Settled;
                    
                case TransactionStatus.SUBMITTED_FOR_SETTLEMENT:
                    return PaymentStatus.Settling;
                    
                case TransactionStatus.SETTLING:
                    return PaymentStatus.Settling;
                    
                case TransactionStatus.VOIDED:
                    return PaymentStatus.Voided;
                    
                case TransactionStatus.PROCESSOR_DECLINED:
                case TransactionStatus.GATEWAY_REJECTED:
                case TransactionStatus.FAILED:
                    return PaymentStatus.Failed;
                    
                default:
                    return PaymentStatus.Pending;
            }
        }
    }
}
