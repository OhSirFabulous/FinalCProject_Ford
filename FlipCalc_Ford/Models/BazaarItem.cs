namespace FlipCalc_Ford.Models
{
    public class BazaarItem
    {
        public string ProductId { get; set; }
        public double BuyPrice { get; set; }
        public double SellPrice { get; set; }

        public BazaarItem(string productId, double buyPrice, double sellPrice)
        {
            ProductId = productId;
            BuyPrice = buyPrice;
            SellPrice = sellPrice;
        }
    }
}
