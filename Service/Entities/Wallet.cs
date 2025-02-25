namespace Service.Entities;

public class Wallet
{
    public string Currency { get; set; }
    public decimal Amount { get; set; }

    public Wallet(string currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }
}