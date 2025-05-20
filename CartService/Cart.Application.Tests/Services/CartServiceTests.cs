// NuGet Packages that would be needed for this test project:
// - Xunit (xunit, xunit.runner.visualstudio)
// - Moq
// - Microsoft.NET.Test.Sdk

using System;
using System.Collections.Generic; // For KeyNotFoundException, List
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Cart.Application.Services;
using Cart.Application.Interfaces;
using Cart.Application.DTOs;
using Cart.Domain.Entities; // For Domain.Entities.Cart and CartItem
using Cart.Domain.Interfaces; // For ICartRepository

namespace Cart.Application.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<ICartRepository> _mockCartRepository;
        private readonly Mock<IProductServiceHttpClient> _mockProductServiceHttpClient;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _mockCartRepository = new Mock<ICartRepository>();
            _mockProductServiceHttpClient = new Mock<IProductServiceHttpClient>();
            // If CartService had ILogger, it would be mocked here too.
            _cartService = new CartService(_mockCartRepository.Object, _mockProductServiceHttpClient.Object);
        }

        [Fact]
        public async Task AddItemAsync_NewItem_ProductFound_AddsItemToCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productDto = new ProductDto { Id = productId, Name = "Test Product", Price = 10.0m, ImageUrl = "test.jpg", Stock = 10 }; // Stock added as it's in ProductDto
            var cartItemDto = new CartItemDto { ProductId = productId, Quantity = 1 }; // UnitPrice is not set here as it comes from ProductService

            _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Domain.Entities.Cart)null);
            _mockProductServiceHttpClient.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync(productDto);

            Domain.Entities.Cart capturedCart = null;
            _mockCartRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()))
                               .Callback<Domain.Entities.Cart>(c => capturedCart = c)
                               .Returns(Task.CompletedTask);

            // Act
            await _cartService.AddItemAsync(userId, cartItemDto);

            // Assert
            _mockProductServiceHttpClient.Verify(p => p.GetProductByIdAsync(productId), Times.Once);
            _mockCartRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _mockCartRepository.Verify(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()), Times.Once);
            
            Assert.NotNull(capturedCart);
            Assert.Equal(userId, capturedCart.UserId);
            Assert.Single(capturedCart.Items);
            var itemInCart = capturedCart.Items.First();
            Assert.Equal(productId, itemInCart.ProductId);
            Assert.Equal(cartItemDto.Quantity, itemInCart.Quantity);
            Assert.Equal(productDto.Price, itemInCart.UnitPrice);
            Assert.Equal(productDto.Name, itemInCart.ProductName);
            Assert.Equal(productDto.ImageUrl, itemInCart.ProductImageUrl);
            Assert.NotEqual(Guid.Empty, itemInCart.Id); // Ensure CartItem.Id is set
        }

        [Fact]
        public async Task AddItemAsync_ExistingItem_ProductFound_UpdatesItemInCart()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var initialQuantity = 2;
            var quantityToAdd = 3;
            var expectedTotalQuantity = initialQuantity + quantityToAdd;

            var existingCartItem = new Domain.Entities.CartItem // Use Domain.Entities.CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = initialQuantity,
                UnitPrice = 10.0m, // Old price
                ProductName = "Old Product Name",
                ProductImageUrl = "old_image.jpg"
            };
            var existingCart = new Domain.Entities.Cart // Use Domain.Entities.Cart
            {
                UserId = userId,
                Items = new List<Domain.Entities.CartItem> { existingCartItem },
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var updatedProductDto = new ProductDto { Id = productId, Name = "Updated Product Name", Price = 12.5m, ImageUrl = "updated_image.jpg", Stock = 5 };
            var cartItemDto = new CartItemDto { ProductId = productId, Quantity = quantityToAdd }; // DTO for adding more items

            _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(existingCart);
            _mockProductServiceHttpClient.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync(updatedProductDto);

            Domain.Entities.Cart capturedCart = null;
            _mockCartRepository.Setup(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()))
                               .Callback<Domain.Entities.Cart>(c => capturedCart = c)
                               .Returns(Task.CompletedTask);

            // Act
            await _cartService.AddItemAsync(userId, cartItemDto);

            // Assert
            _mockProductServiceHttpClient.Verify(p => p.GetProductByIdAsync(productId), Times.Once);
            _mockCartRepository.Verify(r => r.GetByUserIdAsync(userId), Times.Once);
            _mockCartRepository.Verify(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()), Times.Once);

            Assert.NotNull(capturedCart);
            Assert.Single(capturedCart.Items); // Should still be one item type
            var itemInCart = capturedCart.Items.First();
            Assert.Equal(productId, itemInCart.ProductId);
            Assert.Equal(expectedTotalQuantity, itemInCart.Quantity);
            Assert.Equal(updatedProductDto.Price, itemInCart.UnitPrice); // Price updated
            Assert.Equal(updatedProductDto.Name, itemInCart.ProductName); // Name updated
            Assert.Equal(updatedProductDto.ImageUrl, itemInCart.ProductImageUrl); // ImageUrl updated
            Assert.Equal(existingCartItem.Id, itemInCart.Id); // ID should remain the same for existing item
            Assert.NotNull(capturedCart.UpdatedAt); // Ensure UpdatedAt is set
        }

        [Fact]
        public async Task AddItemAsync_ProductNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var cartItemDto = new CartItemDto { ProductId = productId, Quantity = 1 };

            _mockProductServiceHttpClient.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync((ProductDto)null);
             _mockCartRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Domain.Entities.Cart)null); // Cart repo might be called before product check

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _cartService.AddItemAsync(userId, cartItemDto));
            Assert.Equal($"Product with ID {productId} not found.", exception.Message);
            
            _mockProductServiceHttpClient.Verify(p => p.GetProductByIdAsync(productId), Times.Once);
            _mockCartRepository.Verify(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()), Times.Never);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", 1, "ProductId must be valid and Quantity must be greater than 0.")] // Empty Guid ProductId
        [InlineData("10000000-0000-0000-0000-000000000000", 0, "ProductId must be valid and Quantity must be greater than 0.")]   // Zero Quantity
        [InlineData("10000000-0000-0000-0000-000000000000", -1, "ProductId must be valid and Quantity must be greater than 0.")] // Negative Quantity
        public async Task AddItemAsync_InvalidInput_ThrowsArgumentException(string productIdString, int quantity, string expectedMessage)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.Parse(productIdString);
            var cartItemDto = new CartItemDto { ProductId = productId, Quantity = quantity };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _cartService.AddItemAsync(userId, cartItemDto));
            Assert.Equal(expectedMessage, exception.Message); // Check specific message if CartService throws it.
            
            _mockProductServiceHttpClient.Verify(p => p.GetProductByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockCartRepository.Verify(r => r.AddOrUpdateAsync(It.IsAny<Domain.Entities.Cart>()), Times.Never);
        }
    }
}
