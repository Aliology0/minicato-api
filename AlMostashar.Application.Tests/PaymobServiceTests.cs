using AlMostashar.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace AlMostashar.Application.Tests;

public class PaymobServiceTests
{
    private const string IntentionUrl = "https://paymob.com/api/intention/";

    [Fact]
    public async Task CreatePaymentIntentionAsync_FailedResponse_ThrowsException()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("{\"error\":\"Bad Request\"}")
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = CreateConfiguration();
        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePaymentIntentionAsync(1, 100m, null, "item", "desc", "First", "Last", "email", "phone"));

        Assert.Contains("BadRequest", ex.Message);
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_EmptyClientSecret_ThrowsException()
    {
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"id\":\"12345\", \"client_secret\":\"\"}") // Empty client_secret
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var config = CreateConfiguration();
        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePaymentIntentionAsync(1, 100m, null, "item", "desc", "First", "Last", "email", "phone"));

        Assert.Contains("empty client_secret", ex.Message);
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_UsesPaymentMethodIdsFromConfig()
    {
        HttpRequestMessage? capturedRequest = null;
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"12345\", \"client_secret\":\"valid_secret\"}")
                };
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var configParams = new Dictionary<string, string>
        {
            { "PaymobSettings:SecretKey", "secret" },
            { "PaymobSettings:IntentionApiUrl", IntentionUrl },
            { "PaymobSettings:CheckoutBaseUrl", "https://checkout" },
            { "PaymobSettings:RedirectionUrl", "https://redirect" },
            { "PaymobSettings:PublicKey", "public" },
            { "PaymobSettings:IntegrationId", "999" },
            { "PaymobSettings:PaymentMethodIds:0", "111" },
            { "PaymobSettings:PaymentMethodIds:1", "222" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();

        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        await service.CreatePaymentIntentionAsync(10, 100m, null, "item", "desc", "First", "Last", "email", "phone");

        Assert.NotNull(capturedRequest);
        var contentString = await capturedRequest.Content!.ReadAsStringAsync();
        var doc = JsonDocument.Parse(contentString);
        var methods = doc.RootElement.GetProperty("payment_methods").EnumerateArray().Select(e => e.GetInt32()).ToList();

        Assert.Equal(2, methods.Count);
        Assert.Contains(111, methods);
        Assert.Contains(222, methods);

        var specialRef = doc.RootElement.GetProperty("special_reference").GetString();
        Assert.NotNull(specialRef);
        Assert.StartsWith("10_", specialRef); // Starts with PaymentId_
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_FallbackToIntegrationId_WhenPaymentMethodIdsMissing()
    {
        HttpRequestMessage? capturedRequest = null;
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                capturedRequest = request;
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":\"12345\", \"client_secret\":\"valid_secret\"}")
                };
            });

        var httpClient = new HttpClient(httpMessageHandlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        // Missing PaymentMethodIds, so it should fallback to IntegrationId (999)
        var config = CreateConfiguration();

        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        await service.CreatePaymentIntentionAsync(10, 100m, null, "item", "desc", "First", "Last", "email", "phone");

        Assert.NotNull(capturedRequest);
        var contentString = await capturedRequest.Content!.ReadAsStringAsync();
        var doc = JsonDocument.Parse(contentString);
        var methods = doc.RootElement.GetProperty("payment_methods").EnumerateArray().Select(e => e.GetInt32()).ToList();

        Assert.Single(methods);
        Assert.Contains(999, methods);

        var specialRef = doc.RootElement.GetProperty("special_reference").GetString();
        Assert.NotNull(specialRef);
        Assert.StartsWith("10_", specialRef);
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_MissingIntegrationId_ThrowsException()
    {
        var configParams = new Dictionary<string, string>
        {
            { "PaymobSettings:SecretKey", "secret" },
            { "PaymobSettings:IntentionApiUrl", IntentionUrl },
            { "PaymobSettings:CheckoutBaseUrl", "https://checkout" },
            { "PaymobSettings:RedirectionUrl", "https://redirect" },
            { "PaymobSettings:PublicKey", "public" }
            // Missing IntegrationId
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePaymentIntentionAsync(10, 100m, null, "item", "desc", "First", "Last", "email", "phone"));

        Assert.Contains("IntegrationId", ex.Message);
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_ZeroIntegrationId_ThrowsException()
    {
        var configParams = new Dictionary<string, string>
        {
            { "PaymobSettings:SecretKey", "secret" },
            { "PaymobSettings:IntentionApiUrl", IntentionUrl },
            { "PaymobSettings:CheckoutBaseUrl", "https://checkout" },
            { "PaymobSettings:RedirectionUrl", "https://redirect" },
            { "PaymobSettings:PublicKey", "public" },
            { "PaymobSettings:IntegrationId", "0" } // Zero
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePaymentIntentionAsync(10, 100m, null, "item", "desc", "First", "Last", "email", "phone"));

        Assert.Contains("IntegrationId", ex.Message);
    }

    [Fact]
    public async Task CreatePaymentIntentionAsync_InvalidPaymentMethodIds_ThrowsException()
    {
        var configParams = new Dictionary<string, string>
        {
            { "PaymobSettings:SecretKey", "secret" },
            { "PaymobSettings:IntentionApiUrl", IntentionUrl },
            { "PaymobSettings:CheckoutBaseUrl", "https://checkout" },
            { "PaymobSettings:RedirectionUrl", "https://redirect" },
            { "PaymobSettings:PublicKey", "public" },
            { "PaymobSettings:IntegrationId", "999" },
            { "PaymobSettings:PaymentMethodIds:0", "111" },
            { "PaymobSettings:PaymentMethodIds:1", "0" } // Invalid ID
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var service = new PaymobService(httpClientFactoryMock.Object, config, NullLogger<PaymobService>.Instance);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreatePaymentIntentionAsync(10, 100m, null, "item", "desc", "First", "Last", "email", "phone"));

        Assert.Contains("PaymentMethodIds", ex.Message);
    }

    private static IConfiguration CreateConfiguration()
    {
        var configParams = new Dictionary<string, string>
        {
            { "PaymobSettings:SecretKey", "secret" },
            { "PaymobSettings:IntentionApiUrl", IntentionUrl },
            { "PaymobSettings:CheckoutBaseUrl", "https://checkout" },
            { "PaymobSettings:RedirectionUrl", "https://redirect" },
            { "PaymobSettings:PublicKey", "public" },
            { "PaymobSettings:IntegrationId", "999" }
        };
        return new ConfigurationBuilder().AddInMemoryCollection(configParams!).Build();
    }
}
