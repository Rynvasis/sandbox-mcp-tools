using System;
using System.Collections.Generic;
using PlatziStore.Domain.Entities;
using PlatziStore.Domain.ValueObjects;
using PlatziStore.Application.DataTransfer;

namespace PlatziStore.Application.Tests.Helpers;

public static class TestDataFactory
{
    public static Merchandise CreateMerchandise(
        int id = 1,
        string title = "Test Product",
        decimal price = 100m,
        string description = "Test Description",
        string slugStr = "test-product",
        int categoryId = 1,
        string categoryName = "Electronics")
    {
        return new Merchandise
        {
            Id = id,
            Title = title,
            Price = MonetaryAmount.From(price),
            Description = description,
            Slug = SlugIdentifier.From(slugStr),
            Category = new ProductGroup
            {
                Id = categoryId,
                Name = categoryName,
                Slug = SlugIdentifier.From(categoryName.ToLower()),
                CoverImage = ImageUrl.From("https://placehold.co/600x400")
            },
            Images = new List<ImageUrl> { ImageUrl.From("https://placehold.co/600x400") }
        };
    }

    public static ProductGroup CreateProductGroup(
        int id = 1,
        string name = "Electronics",
        string slugStr = "electronics")
    {
        return new ProductGroup
        {
            Id = id,
            Name = name,
            Slug = SlugIdentifier.From(slugStr),
            CoverImage = ImageUrl.From("https://placehold.co/600x400")
        };
    }

    public static StoreCustomer CreateStoreCustomer(
        int id = 1,
        string name = "John Doe",
        string emailStr = "john@example.com",
        string role = "customer")
    {
        return new StoreCustomer
        {
            Id = id,
            DisplayName = name,
            Email = EmailAddress.From(emailStr),
            Role = role,
            Avatar = ImageUrl.From("https://placehold.co/200x200")
        };
    }

    public static CategoryPayload CreateCategoryPayload(string name = "Electronics")
    {
        return new CategoryPayload
        {
            Name = name,
            Image = "https://placehold.co/600x400"
        };
    }

    public static CatalogItemPayload CreateCatalogItemPayload(string title = "Test Product", decimal price = 100m)
    {
        return new CatalogItemPayload
        {
            Title = title,
            Price = price,
            Description = "Test description",
            CategoryId = 1,
            Images = new[] { "https://placehold.co/600x400" }
        };
    }
}
