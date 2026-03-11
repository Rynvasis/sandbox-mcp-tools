using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;
using System.Text;

namespace PlatziStore.Host.Formatting;

public static class ResponseFormatter
{
    public static string FormatOutcome<T>(OperationOutcome<T> outcome, Func<T, string> formatData)
    {
        if (!outcome.IsSuccess)
        {
            return $"Error: {outcome.ErrorMessage}";
        }

        return outcome.Data is not null 
            ? formatData(outcome.Data) 
            : "Success";
    }

    public static string FormatProductSummary(CatalogItemSummary summary)
    {
        return $"Product #{summary.Id}: {summary.Title} - ${summary.Price} [{summary.CategoryName}] (slug: {summary.Slug})";
    }

    public static string FormatProductList(PagedCollection<CatalogItemSummary> list)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Total} products (showing {list.Items.Count} items, offset {list.Offset}):");
        foreach (var item in list.Items)
        {
            sb.AppendLine("- " + FormatProductSummary(item));
        }
        return sb.ToString().TrimEnd();
    }

    public static string FormatProductDetail(CatalogItemDetail detail)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Product #{detail.Id}: {detail.Title}");
        sb.AppendLine($"Price: ${detail.Price}");
        sb.AppendLine($"Category: {detail.CategoryName} (#{detail.CategoryId}, slug: {detail.CategorySlug})");
        sb.AppendLine($"Slug: {detail.Slug}");
        sb.AppendLine("Description:");
        sb.AppendLine(detail.Description);
        
        if (detail.ImageUrls.Any())
        {
            sb.AppendLine("Images:");
            foreach (var img in detail.ImageUrls)
            {
                sb.AppendLine($"- {img}");
            }
        }
        return sb.ToString().TrimEnd();
    }

    public static string FormatCategory(CategorySummary category)
    {
        return $"Category #{category.Id}: {category.Name} (slug: {category.Slug}, image: {category.ImageUrl})";
    }


    public static string FormatCategoryList(IReadOnlyList<CategorySummary> list)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Count} categories:");
        foreach (var category in list)
        {
            sb.AppendLine("- " + FormatCategory(category));
        }
        return sb.ToString().TrimEnd();
    }

    public static string FormatCustomerProfile(CustomerProfile profile)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Customer #{profile.Id}: {profile.DisplayName}");
        sb.AppendLine($"Email: {profile.Email}");
        sb.AppendLine($"Role: {profile.Role}");
        if (!string.IsNullOrEmpty(profile.AvatarUrl))
        {
            sb.AppendLine($"Avatar: {profile.AvatarUrl}");
        }
        return sb.ToString().TrimEnd();
    }

    public static string FormatCustomerList(IReadOnlyList<CustomerProfile> list)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Found {list.Count} customers:");
        foreach (var profile in list)
        {
            sb.AppendLine("- " + FormatCustomerProfile(profile));
        }
        return sb.ToString().TrimEnd();
    }

    public static string FormatTokenPair(AuthTokenPair tokens)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Authentication Successful");
        sb.AppendLine($"Access Token: {tokens.AccessToken}");
        sb.AppendLine($"Refresh Token: {tokens.RefreshToken}");
        return sb.ToString().TrimEnd();
    }
}
