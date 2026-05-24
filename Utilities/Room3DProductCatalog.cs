using WebActionResults.Models;

namespace WebActionResults.Utilities;

public static class Room3DProductCatalog
{
    public static readonly IReadOnlyDictionary<string, string> ProductNamesByRoomId =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["dragon-01"] = "Shenron Guardian Dragon",
            ["sofa-01"] = "Modern L-Shaped Sofa",
            ["sofa-02"] = "Velvet Sectional Sofa",
            ["sofa-03"] = "Nordic 3-Seater Sofa",
            ["table-01"] = "Minimalist Coffee Table",
            ["table-02"] = "Marble Top Table",
            ["table-03"] = "Glass Top Side Table",
            ["chair-01"] = "Classic Leather Armchair",
            ["chair-02"] = "Curved Accent Chair",
            ["chair-03"] = "Chaise Lounge Chair",
            ["chair-04"] = "Ergonomic Office Chair",
            ["stool-01"] = "Storage Ottoman",
            ["lamp-01"] = "Corner Floor Lamp",
            ["lamp-02"] = "Metal Floor Lamp",
            ["lamp-03"] = "Desk Lamp LED",
            ["plant-01"] = "Artificial Plant Tree",
            ["plant-02"] = "Artificial Plant Tree (Tall)",
            ["plant-03"] = "Artificial Plant Tree (Mini)",
            ["rug-01"] = "Candle Holder Set",
            ["rug-02"] = "Candle Holder Set",
            ["rug-03"] = "Candle Holder Set",
            ["cabinet-01"] = "Oak Wood TV Stand",
            ["cabinet-02"] = "Filing Cabinet 4-drawer",
            ["cabinet-03"] = "Mobile Pedestal",
            ["decor-01"] = "Abstract Canvas Art",
            ["decor-02"] = "Ceramic Vase Large",
            ["decor-03"] = "Sculpture Statue",
            ["shelf-01"] = "Wall Shelf Unit",
            ["shelf-02"] = "Modular Bookshelf"
        };

    public static bool TryGetRoomProductId(Product product, out string roomProductId)
        => TryGetRoomProductId(product.ProductName, out roomProductId);

    public static bool TryGetRoomProductId(string? productName, out string roomProductId)
    {
        roomProductId = string.Empty;
        if (string.IsNullOrWhiteSpace(productName))
        {
            return false;
        }

        var match = ProductNamesByRoomId.FirstOrDefault(pair =>
            string.Equals(pair.Value, productName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrWhiteSpace(match.Key))
        {
            return false;
        }

        roomProductId = match.Key;
        return true;
    }
}
