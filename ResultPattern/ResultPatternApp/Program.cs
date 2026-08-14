namespace ResultPatternApp;

internal abstract class Program
{
    private static void Main(string[] args)
    {
        var bank = new BankTransfer();

        var result = bank.TryOpenAccount(200)
            .Bind(x => x.TryWithdraw(300))
            .Match(success => "Transfer Completed",
                errors =>
                    $"Transfer Failed {string.Join(Environment.NewLine, errors)}");

        Console.WriteLine(result);
    }
}