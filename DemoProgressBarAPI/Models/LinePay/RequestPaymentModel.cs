using System.Text.Json.Serialization;
using DemoProgressBarAPI.Models.Enums.LinePay;

namespace DemoProgressBarAPI.Models.LinePay
{
    public class RequestPaymentModel
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("packages")]
        public IEnumerable<Package> Packages { get; set; }

        [JsonPropertyName("redirectUrls")]
        public RedirectUrls RedirectUrls { get; set; }
    }

    public class RedirectUrls
    {
        [JsonPropertyName("confirmUrl")]
        public string ConfirmUrl { get; set; }

        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonPropertyName("confirmUrlType")]
        public string ConfirmUrlType { get; set; } = ConfirmUrlTypeEnum.CLIENT.ToString();

        [JsonPropertyName("appPackageName")]
        public string AppPackageName { get; set; }
    }

    public class Package
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("products")]
        public IEnumerable<Product> Products { get; set; }
    }

    public class Product
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}
