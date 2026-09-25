using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AlMostashar.Infrastructure.Services
{
    public class PaymobService: IPaymentService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymobService> _logger;

        public PaymobService(IHttpClientFactory httpClient, IConfiguration configuration, ILogger<PaymobService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaymentResultDto> CreatePaymentIntentionAsync(
            int paymentId,
            decimal amountInEgp,
            int? expirationMinutes,
            string itemName,
            string itemDescription,
            string? clientFirstName,
            string? clientLastName,
            string? clientEmail,
            string? clientPhone)
        {
            // ── Validate required configuration values ──
            string secretKey = _configuration.GetValue<string>("PaymobSettings:SecretKey")
                ?? throw new InvalidOperationException("PaymobSettings:SecretKey is not configured.");
            string intentionApiUrl = _configuration.GetValue<string>("PaymobSettings:IntentionApiUrl")
                ?? throw new InvalidOperationException("PaymobSettings:IntentionApiUrl is not configured.");
            string checkoutBaseUrl = _configuration.GetValue<string>("PaymobSettings:CheckoutBaseUrl")
                ?? throw new InvalidOperationException("PaymobSettings:CheckoutBaseUrl is not configured.");
            string redirectionUrl = _configuration.GetValue<string>("PaymobSettings:RedirectionUrl")
                ?? throw new InvalidOperationException("PaymobSettings:RedirectionUrl is not configured.");
            string publicKey = _configuration.GetValue<string>("PaymobSettings:PublicKey")
                ?? throw new InvalidOperationException("PaymobSettings:PublicKey is not configured.");
            int? integrationId = _configuration.GetValue<int?>("PaymobSettings:IntegrationId");

            if (!integrationId.HasValue || integrationId.Value <= 0)
                throw new InvalidOperationException("PaymobSettings:IntegrationId must be configured and greater than zero.");

            // ── Read payment method IDs from configuration ──
            var paymentMethodIds = _configuration.GetSection("PaymobSettings:PaymentMethodIds")
                .Get<int[]>();

            int[] paymentMethods;
            if (paymentMethodIds is { Length: > 0 })
            {
                if (paymentMethodIds.Any(id => id <= 0))
                    throw new InvalidOperationException("PaymobSettings:PaymentMethodIds must contain only positive IDs.");
                paymentMethods = paymentMethodIds;
            }
            else
            {
                // Fallback: if PaymentMethodIds not configured, use IntegrationId only
                paymentMethods = new[] { integrationId.Value };
            }

            var payload = new
            {
                amount = (int)(amountInEgp * 100), // Convert to cents
                currency = "EGP",
                payment_methods = paymentMethods,

                // Provide items for clarity and compatibility
                items = new[]
                {
                    new
                    {
                        name = itemName,
                        amount = (int)(amountInEgp * 100),
                        description = itemDescription,
                        quantity = 1
                    }
                },

                // Pass real client data, and dummy data for physical addresses
                billing_data = new
                {
                    first_name = clientFirstName ?? "Client",
                    last_name = clientLastName ?? "Client",
                    email = clientEmail,
                    phone_number = clientPhone,
                    apartment = "NA",
                    street = "NA",
                    building = "NA",
                    city = "NA",
                    country = "EG",
                    floor = "NA",
                    state = "NA"
                },

                // Payment.Id as the primary webhook correlation key (with Guid to allow retries)
                special_reference = $"{paymentId}_{Guid.NewGuid():N}",
                expiration = expirationMinutes,

                // The URL Flutter will intercept
                redirection_url = redirectionUrl
            };

            // Serialize the payload to JSON
            string jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var client = _httpClient.CreateClient();
            // Set the Authorization header directly
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Token {secretKey}");

            // Send POST request to the Intention API
            HttpResponseMessage response = await client.PostAsync(intentionApiUrl, content);

            // ── Handle failed Paymob response ──
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Paymob intention API failed. StatusCode={StatusCode}, Body={Body}, PaymentId={PaymentId}.",
                    response.StatusCode, errorBody, paymentId);
                throw new InvalidOperationException(
                    $"Paymob intention API returned {response.StatusCode}. See logs for details.");
            }

            // Read and return the generated Checkout URL or Client Secret
            var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>();

            _logger.LogDebug("Paymob intention API response for PaymentId={PaymentId}: {Response}",
                paymentId, responseJson?.ToJsonString());

            string clientKey = responseJson?["client_secret"]?.ToString() ?? "";
            string intentionId = responseJson?["id"]?.ToString() ?? "";

            // ── Validate client_secret is present ──
            if (string.IsNullOrWhiteSpace(clientKey))
            {
                _logger.LogError(
                    "Paymob returned empty client_secret for PaymentId={PaymentId}. IntentionId={IntentionId}, Response={Response}.",
                    paymentId, intentionId, responseJson?.ToJsonString());
                throw new InvalidOperationException(
                    "Paymob returned an empty client_secret. Cannot generate checkout URL.");
            }

            string paymentUrl = $"{checkoutBaseUrl}?publicKey={publicKey}&clientSecret={clientKey}";

            return new PaymentResultDto(paymentUrl, clientKey, intentionId);
        }

        public async Task<RefundPaymentResultDto> RefundPaymentAsync(
    int transactionId,
    decimal amountCents)
        {
            string refundUrl = _configuration.GetValue<string>("PaymobSettings:RefundApiUrl")
                ?? throw new InvalidOperationException("PaymobSettings:RefundApiUrl is not configured.");

            string secretKey = _configuration.GetValue<string>("PaymobSettings:SecretKey")
                ?? throw new InvalidOperationException("PaymobSettings:SecretKey is not configured.");

            var client = _httpClient.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Token {secretKey}");

            var payload = new
            {
                transaction_id = transactionId,
                amount_cents = amountCents
            };

            string jsonContent = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(refundUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Paymob Refund API failed. StatusCode={StatusCode}, Body={Body}, TransactionId={TransactionId}.",
                    response.StatusCode,
                    responseBody,
                    transactionId);

                throw new InvalidOperationException(
                    $"Paymob Refund API returned {response.StatusCode}. See logs for details.");
            }

            JsonObject? responseJson;

            try
            {
                responseJson = JsonSerializer.Deserialize<JsonObject>(responseBody);
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Paymob Refund API returned invalid JSON. TransactionId={TransactionId}, Body={Body}.",
                    transactionId,
                    responseBody);

                throw new InvalidOperationException("Paymob Refund API returned invalid JSON.");
            }

            if (responseJson is null)
                throw new InvalidOperationException("Paymob Refund API returned empty response.");

            var order = responseJson["order"]?.AsObject();
            var data = responseJson["data"]?.AsObject();

            var result = new RefundPaymentResultDto
            {
                RefundTransactionId = responseJson["id"]?.GetValue<int>() ?? 0,
                OriginalTransactionId = responseJson["parent_transaction"]?.GetValue<int>() ?? transactionId,
                OrderId = order?["id"]?.GetValue<int>() ?? 0,

                AmountCents = responseJson["amount_cents"]?.GetValue<decimal>() ?? amountCents,

                Success = responseJson["success"]?.GetValue<bool>() ?? false,
                Pending = responseJson["pending"]?.GetValue<bool>() ?? true,
                IsRefund = responseJson["is_refund"]?.GetValue<bool>() ?? false,
                ErrorOccured = responseJson["error_occured"]?.GetValue<bool>() ?? true,

                GatewayMessage = data?["message"]?.ToString(),

                RawResponse = responseBody
            };

            var isRefundSucceeded =
                result.Success &&
                !result.Pending &&
                result.IsRefund &&
                !result.ErrorOccured;

            if (!isRefundSucceeded)
            {
                _logger.LogError(
                    "Paymob Refund API returned unsuccessful refund response. " +
                    "OriginalTransactionId={OriginalTransactionId}, RefundTransactionId={RefundTransactionId}, " +
                    "AmountCents={AmountCents}, ExpectedAmountCents={ExpectedAmountCents}, " +
                    "Success={Success}, Pending={Pending}, IsRefund={IsRefund}, ErrorOccured={ErrorOccured}, " +
                    "GatewayMessage={GatewayMessage}, Body={Body}",
                    result.OriginalTransactionId,
                    result.RefundTransactionId,
                    result.AmountCents,
                    amountCents,
                    result.Success,
                    result.Pending,
                    result.IsRefund,
                    result.ErrorOccured,
                    result.GatewayMessage,
                    responseBody);

                throw new InvalidOperationException("Paymob refund was not successful.");
            }

            return result;
        }
    }
}
