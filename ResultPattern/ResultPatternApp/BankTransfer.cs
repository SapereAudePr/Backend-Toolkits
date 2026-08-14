namespace ResultPatternApp;

public class BankTransfer(decimal balance)
{
    private decimal _balance = balance;

    public Result TryWithdraw(decimal amount)
    {
        if (amount <= 0)
        {
            return Result.Failure("amount", "There's not enough balance");
        }

        if (amount > _balance)
        {
            return Result.Failure("amount",
                $"Withdraw failed: requested amount {amount:C}, but the available balance is {_balance:C}.");
        }

        _balance -= amount;
        Console.WriteLine($"Withdraw amount {amount} and the remaining balance is {_balance}");
        return Result.Success();
    }
}