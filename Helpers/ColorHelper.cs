namespace WebActionResults.Helpers;

public static class ColorHelper
{
    private static readonly Dictionary<string, string> ColorMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Grays & Neutrals
        { "Gray", "#6c757d" },
        { "Grey", "#6c757d" },
        { "Black", "#212529" },
        { "White", "#ffffff" },
        { "Cream", "#fffdd0" },
        { "Tan", "#d2b48c" },
        { "Beige", "#f5f5dc" },
        { "Brown", "#8b4513" },
        { "Walnut", "#5d432c" },
        { "Oak", "#c19a6b" },
        { "Natural", "#d4a574" },

        // Blues
        { "Navy Blue", "#000080" },
        { "Navy", "#000080" },
        { "Blue", "#0d6efd" },
        { "Sky Blue", "#87ceeb" },
        { "Royal Blue", "#4169e1" },
        { "Light Blue", "#add8e6" },
        { "Steel Blue", "#4682b4" },
        { "Dark Blue", "#00008b" },

        // Greens
        { "Green", "#198754" },
        { "Emerald", "#50c878" },
        { "Sage", "#9dc183" },
        { "Forest Green", "#228b22" },
        { "Olive", "#808000" },
        { "Mint", "#98ff98" },

        // Reds & Pinks
        { "Red", "#dc3545" },
        { "Burgundy", "#800020" },
        { "Crimson", "#dc143c" },
        { "Pink", "#ffc0cb" },
        { "Rose", "#ff007f" },
        { "Magenta", "#ff00ff" },

        // Yellows & Oranges
        { "Yellow", "#ffc107" },
        { "Mustard", "#ffdb58" },
        { "Gold", "#ffd700" },
        { "Orange", "#fd7e14" },
        { "Peach", "#ffcba4" },

        // Purples
        { "Purple", "#6f42c1" },
        { "Violet", "#8b00ff" },
        { "Lavender", "#e6e6fa" },
        { "Turquoise", "#40e0d0" },

        // Metallics
        { "Silver", "#c0c0c0" },
        { "Chrome", "#e8e8e8" },
        { "Brass", "#b5a642" },
        { "Bronze", "#cd7f32" },
        { "Copper", "#b87333" },

        // Special Finishes
        { "Teak", "#b8860b" },
        { "Mahogany", "#c04000" },
        { "Ebony", "#555d50" },
        { "Cherry", "#deb887" },

        // Marble & Stone
        { "White Marble", "#f5f5f5" },
        { "Black Marble", "#36454f" },
        { "Gray Marble", "#808080" },
        { "Concrete", "#808080" },

        // Velvets & Fabrics
        { "Dark Velvet", "#301934" },
        { "Velvet", "#7b3f61" },

        // Other
        { "Abstract Mix", "#964b00" },
        { "Black & White", "#000000" },
        { "Nature", "#228b22" }
    };

    public static string ToCssColor(string? colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName))
            return "#6c757d";

        if (ColorMap.TryGetValue(colorName, out var hex))
            return hex;

        foreach (var kvp in ColorMap)
        {
            if (colorName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        var firstWord = colorName.Split(' ')[0];
        foreach (var kvp in ColorMap)
        {
            if (kvp.Key.Equals(firstWord, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return "#6c757d";
    }
}