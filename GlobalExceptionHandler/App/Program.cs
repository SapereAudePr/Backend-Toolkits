namespace App;

public class Program
{
    static void Main(string[] args)
    {
        // var result = Divider(int.MinValue, -1);
        // Console.WriteLine(result);
        // var result1 = Divider(2, 0);
        // Console.WriteLine(result1);

        // try
        // {
        //     Withdraw(100, 890);
        //     Withdraw(200, 100);
        // }
        // catch (InsufficientFundsException e)
        // {
        //     Console.WriteLine(e.Message);
        //     throw;
        // }
    }

    private static void Withdraw(decimal balance, decimal amount)
    {
        if (amount > balance)
            throw new InsufficientFundsException(amount - balance);

        Console.WriteLine($"Withdrew: {amount}. Remaining balance: {balance - amount}");
    }


    // private static int Divider(int first, int second)
    // {
    //     try
    //     {
    //         return first / second;
    //     }
    //     catch (DivideByZeroException e)
    //     {
    //         Console.WriteLine(e.Message);
    //         throw;
    //     }
    //     catch (OverflowException e)
    //     {
    //         Console.WriteLine(e.Message);
    //         throw;
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e.Message);
    //         throw;
    //     }
    // }
}