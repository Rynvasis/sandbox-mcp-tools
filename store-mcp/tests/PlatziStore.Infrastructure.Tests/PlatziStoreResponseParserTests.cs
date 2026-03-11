using System.Collections.Generic;
using System.Text.Json;
using FluentAssertions;
using PlatziStore.Domain.Entities;
using PlatziStore.Infrastructure.ApiClients;
using Xunit;

namespace PlatziStore.Infrastructure.Tests;

public class PlatziStoreResponseParserTests
{
    private readonly PlatziStoreResponseParser _parser;

    public PlatziStoreResponseParserTests()
    {
        _parser = new PlatziStoreResponseParser();
    }

    [Fact]
    public void ParseProduct_ShouldReturnMerchandise_WhenValidJson()
    {
        var json = """
        {
            "id": 1,
            "title": "Classic White T-Shirt",
            "price": 25,
            "description": "A comfortable classic white t-shirt.",
            "images": [
                "https://example.com/images/shirt1.jpg"
            ],
            "category": {
                "id": 2,
                "name": "Clothing",
                "image": "https://example.com/images/clothing.jpg",
                "slug": "clothing"
            },
            "slug": "classic-white-t-shirt"
        }
        """;

        var product = _parser.ParseProduct(json);

        product.Should().NotBeNull();
        product.Id.Should().Be(1);
        product.Title.Should().Be("Classic White T-Shirt");
        product.Price.Value.Should().Be(25m);
        product.Description.Should().Be("A comfortable classic white t-shirt.");
        product.Images.Should().HaveCount(1);
        product.Images[0].Value.Should().Be("https://example.com/images/shirt1.jpg");
        product.Category.Should().NotBeNull();
        product.Category.Id.Should().Be(2);
        product.Category.Name.Should().Be("Clothing");
    }

    [Fact]
    public void ParseCategory_ShouldReturnProductGroup_WhenValidJson()
    {
        var json = """
        {
            "id": 5,
            "name": "Electronics",
            "image": "https://example.com/images/elec.jpg",
            "slug": "electronics"
        }
        """;

        var category = _parser.ParseCategory(json);

        category.Should().NotBeNull();
        category.Id.Should().Be(5);
        category.Name.Should().Be("Electronics");
        category.Slug.Value.Should().Be("electronics");
        category.CoverImage.Value.Should().Be("https://example.com/images/elec.jpg");
    }

    [Fact]
    public void ParseProduct_ShouldThrowJsonException_WhenMalformedJson()
    {
        var json = "{ malformed_json: true ";

        var action = () => _parser.ParseProduct(json);

        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void ParseProducts_ShouldReturnEmptyList_WhenEmptyJsonArray()
    {
        var json = "[]";

        var products = _parser.ParseProducts(json);

        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }
    
    [Fact]
    public void ParseCategories_ShouldReturnCategoriesList_WhenValidJson()
    {
        var json = """
        [
            {
                "id": 1,
                "name": "Category 1",
                "image": "https://example.com/img1.jpg",
                "slug": "cat-1"
            }
        ]
        """;
        
        var categories = _parser.ParseCategories(json);
        
        categories.Should().HaveCount(1);
        categories[0].Id.Should().Be(1);
    }
}
