namespace ResultPatternApp;

class Program
{
    static void Main(string[] args)
    {
        var bank = new BankTransfer(200);
        if (!bank.TryWithdraw(190, out var reason))
        {
            Console.WriteLine(reason);
        }
    }
}