namespace Domain.Entities;

public class Account
{
    public int Id { get; }
    public string OwnerName { get; }
    public decimal Balance { get; private set; }

    public Account(string ownerName, int id)
    {
        OwnerName = ownerName;
        Id = id;
        Balance = 0;
    }

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        Balance -= amount;
    }

    public override string ToString() =>
        $"Account {Id}: {OwnerName}, Balance: {Balance:C}";
}