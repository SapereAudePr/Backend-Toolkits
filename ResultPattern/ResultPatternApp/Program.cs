namespace ResultPatternApp;

class Program
{
    static void Main(string[] args)
    {
        var bank = new BankTransfer(200);
        var transfer = bank.TryWithdraw(210);
        transfer.CheckResult();
    }
}