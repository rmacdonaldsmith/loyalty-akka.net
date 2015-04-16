namespace LoyaltyService
{
    public class Gift
    {
        public int Amount { get; private set; }
        public string CCY { get; private set; }

        public Gift(int amount, string ccy)
        {
            Amount = amount;
            CCY = ccy;
        }
    }
}
