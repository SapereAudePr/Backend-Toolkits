namespace ResultPatternApp;

public class BankTransfer
{
    private decimal Balance { get; set; }

    public Result<BankTransfer> TryOpenAccount(decimal initialDeposit)
    {
        if (initialDeposit < 0)
            return Result<BankTransfer>.Failure([
                new ErrorMessage("initialDeposit", "Initial deposit can not be lower than 0")
            ]);

        Balance = initialDeposit;

        return Result<BankTransfer>.Success(this);
    }

    public Result<BankTransfer> TryWithdraw(decimal amount)
    {
        if (amount > Balance)
            return Result<BankTransfer>.Failure
                ([new ErrorMessage("amount", "There's not enough balance")]);

        Balance -= amount;

        Console.WriteLine($"{amount} has been withdrawn, current balance: {Balance}");
        
        return Result<BankTransfer>.Success(this);
    }
}