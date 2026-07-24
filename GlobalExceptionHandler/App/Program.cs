namespace App;

class Program
{
    static void Main(string[] args)
    {
        var result = Divider(int.MinValue, -1);
        Console.WriteLine(result);
        var result1 = Divider(2, 0);
        Console.WriteLine(result1);
    }

    private static int Divider(int first, int second)
    {
        try
        {
            return first / second;
        }
        catch (DivideByZeroException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        catch (OverflowException e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}