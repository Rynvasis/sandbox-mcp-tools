using System.Text.RegularExpressions;

namespace PlatziStore.Shared.Utilities;

public static class SlugGenerator
{
    public static string Generate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var slug = text.ToLowerInvariant().Trim();
        
        // Remove invalid chars
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");
        
        // Remove consecutive hyphens
        slug = Regex.Replace(slug, @"-+", "-");
        
        return slug.Trim('-');
    }
}
