namespace ResultPatternApp;

public class BankTransfer(decimal balance)
{
    private decimal _balance = balance;

    public bool TryWithdraw(decimal amount, out string reason)
    {
        if (amount <= 0)
        {
            reason = "Amount can not be equal or lower to 0";
            return false;
        }

        if (amount > _balance)
        {
            reason = $"Withdraw failed: requested amount {amount:C}, but the available balance is {_balance:C}.";
            return false;
        }

        _balance -= amount;
        Console.WriteLine($"Withdraw amount {amount} and the remaining balance is {_balance}");
        reason = string.Empty;
        return true;
    }
}