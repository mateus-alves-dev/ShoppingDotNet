using System.Net.Http.Json;
using Cart.Application.DTOs;
using Cart.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cart.Infrastructure.HttpClients;

public class ProductServiceHttpClient : IProductServiceHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductServiceHttpClient> _logger;

    public ProductServiceHttpClient(HttpClient httpClient, ILogger<ProductServiceHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid productId)
    {
        try
        {
            // The actual path will be like "api/Product/{productId}"
            // The base address "http://product-api:8080/" will be configured during HttpClient registration
            var response = await _httpClient.GetAsync($"api/Product/{productId}");

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                {
                    _logger.LogWarning("Product service returned success but no content for product ID {ProductId}.", productId);
                    return null; 
                }
                try 
                {
                    var product = await response.Content.ReadFromJsonAsync<ProductDto>();
                    return product;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize product response for product ID {ProductId}.", productId);
                    return null;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Product with ID {ProductId} not found in ProductService.", productId);
                return null;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get product from ProductService. Status: {StatusCode}, ProductId: {ProductId}, Response: {Response}", response.StatusCode, productId, errorContent);
                return null; // Or throw a custom exception
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling ProductService for product ID {ProductId}.", productId);
            return null; // Or throw a custom exception
        }
    }
}
