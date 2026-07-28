namespace App;

public class InsufficientFundsException(decimal shortfall)
    : Exception($"Insufficient funds, you're short by {shortfall}")
{
}