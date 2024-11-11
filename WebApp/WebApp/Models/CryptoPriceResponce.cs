namespace WebApp.Models
{
    public class CryptoPriceResponce
    {
        public string Ticker { get; set; } = string.Empty;
        public List<CryptoPrice> PriceData { get; set; }
    }
}
