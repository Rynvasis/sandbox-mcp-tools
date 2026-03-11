using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PlatziStore.Domain.Exceptions;
using PlatziStore.Infrastructure.ApiClients;
using PlatziStore.Infrastructure.ApiClients.Dtos;
using PlatziStore.Infrastructure.Configuration;
using PlatziStore.Infrastructure.Observability;
using PlatziStore.Infrastructure.Tests.Helpers;
using PlatziStore.Shared.Exceptions;
using Xunit;

namespace PlatziStore.Infrastructure.Tests;

public class PlatziStoreGatewayTests
{
    private PlatziStoreGateway CreateGateway(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
    {
        var handler = new MockHttpMessageHandler(handlerFunc);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.escuelajs.co/api/v1/") };
        
        var parser = new PlatziStoreResponseParser();
        var gatewayLogger = new Mock<ILogger<PlatziStoreGateway>>().Object;
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var structuredLoggerLogger = new Mock<ILogger<StructuredEventLogger>>().Object;
        var structuredLogger = new StructuredEventLogger(structuredLoggerLogger, options, metrics);

        return new PlatziStoreGateway(client, parser, gatewayLogger, metrics, structuredLogger);
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsParsedProducts_When200Response()
    {
        var json = """
        [
            {
                "id": 1,
                "title": "Test",
                "price": 10,
                "description": "Desc",
                "images": ["https://ex.com/1.jpg"],
                "category": { "id": 1, "name": "Cat", "image": "https://ex.com/1.jpg", "slug": "cat" },
                "slug": "test"
            }
        ]
        """;

        var gateway = CreateGateway(req =>
        {
            req.RequestUri!.PathAndQuery.Should().Be("/api/v1/products");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });

        var result = await gateway.GetAllProductsAsync();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task GetProductByIdAsync_ThrowsEntityNotFound_When404Response()
    {
        var gateway = CreateGateway(req => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Not found") });

        Func<Task> act = async () => await gateway.GetProductByIdAsync(99);

        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage("*99*"); // fallback entityId is used in the throw msg or the actual id 99
    }

    [Fact]
    public async Task AnyMethod_ThrowsExternalServiceException_When500Response()
    {
        var gateway = CreateGateway(req => new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Server failure") });

        Func<Task> act = async () => await gateway.GetAllCategoriesAsync();

        var ex = await act.Should().ThrowAsync<ExternalServiceException>();
        ex.WithMessage("*Server error: Server failure*");
    }

    [Fact]
    public async Task FilterProductsAsync_ConstructsCorrectQueryString()
    {
        var json = "[]";
        var gateway = CreateGateway(req =>
        {
            // Verify correct query string is constructed
            req.RequestUri!.PathAndQuery.Should().Contain("title=Test");
            req.RequestUri!.PathAndQuery.Should().Contain("priceMin=10");
            req.RequestUri!.PathAndQuery.Should().Contain("categoryId=5");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });

        await gateway.FilterProductsAsync(title: "Test", priceMin: 10m, categoryId: 5);
    }

    [Fact]
    public async Task CreateProductAsync_SerializesBodyCorrectly()
    {
        var requestDto = new CreateProductApiRequest
        {
            Title = "New Product",
            Price = 50,
            Description = "A new product",
            CategoryId = 3,
            Images = new List<string> { "https://ex.com/img.jpg" }
        };

        var responseJson = """
        {
            "id": 100,
            "title": "New Product",
            "price": 50,
            "description": "A new product",
            "images": ["https://ex.com/img.jpg"],
            "category": { "id": 3, "name": "Cat", "image": "https://ex.com/1.jpg", "slug": "cat" },
            "slug": "new-product"
        }
        """;

        var gateway = CreateGateway(req =>
        {
            req.Method.Should().Be(HttpMethod.Post);
            
            // Read body synchronously just for testing assertion
            var body = req.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            var element = JsonDocument.Parse(body).RootElement;
            
            element.GetProperty("title").GetString().Should().Be("New Product");
            element.GetProperty("price").GetDecimal().Should().Be(50m);
            element.GetProperty("categoryId").GetInt32().Should().Be(3);
            
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent(responseJson) };
        });

        var product = await gateway.CreateProductAsync(requestDto);

        product.Should().NotBeNull();
        product.Id.Should().Be(100);
        product.Title.Should().Be("New Product");
    }
}
