using System.Text.Json.Serialization;

namespace DietHelper.Server.Models
{
    public class OFFProduct
    {
        public int Status { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Barcode { get; set; } = string.Empty;

        [JsonPropertyName("product")]
        public ProductInfo ProductInfo { get; set; } = new();
    }
    public class ProductInfo
    {
        [JsonPropertyName("product_name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("brands")]
        public string Brand { get; set; } = string.Empty;

        [JsonPropertyName("nutriments")]
        public Nutriments Nutriments { get; set; } = new();
    }

    public class Nutriments
    {
        [JsonPropertyName("energy-kcal_100g")]
        public double Calories { get; set; }

        [JsonPropertyName("proteins_100g")]
        public double Protein { get; set; }

        [JsonPropertyName("fat_100g")]
        public double Fat { get; set; }

        [JsonPropertyName("carbohydrates_100g")]
        public double Carbs { get; set; }
    }
}
