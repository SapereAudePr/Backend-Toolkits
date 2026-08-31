using Microsoft.AspNetCore.Identity;
using PasswordHasher.Security.Hashing;

namespace PasswordHasher;

class Program
{
    static void Main(string[] args)
    {
        var custom = new Pbkdf2PasswordHasher();
        var builtIn = new PasswordHasher<object>();
 
        const string password = "MyPassword123";
        const string wrongPassword = "WrongGuess";
 
        Console.WriteLine("=== Hand-rolled PBKDF2 hasher ===");
 
        var customHash1 = custom.Hash(password);
        var customHash2 = custom.Hash(password);
 
        Console.WriteLine($"Hash #1: {customHash1}");
        Console.WriteLine($"Hash #2: {customHash2}");
        Console.WriteLine($"Same output both times? {customHash1 == customHash2}  (expect: False — different salt each time)");
        Console.WriteLine();
 
        Console.WriteLine($"Verify with correct password:  {custom.Verify(customHash1, password)}   (expect: True)");
        Console.WriteLine($"Verify with wrong password:     {custom.Verify(customHash1, wrongPassword)}   (expect: False)");
        Console.WriteLine();
 
        Console.WriteLine("=== .NET's built-in PasswordHasher<T> ===");
 
        var builtInHash = builtIn.HashPassword(null!, password);
        Console.WriteLine($"Hash: {builtInHash}");
 
        var builtInResult = builtIn.VerifyHashedPassword(null!, builtInHash, password);
        Console.WriteLine($"Verify with correct password: {builtInResult}   (expect: Success)");
 
        var builtInWrongResult = builtIn.VerifyHashedPassword(null!, builtInHash, wrongPassword);
        Console.WriteLine($"Verify with wrong password:    {builtInWrongResult}   (expect: Failed)");
        Console.WriteLine();
 
        Console.WriteLine("Both implementations do the same conceptual thing:");
        Console.WriteLine("generate a random salt, run PBKDF2 many times, pack iterations+salt+key");
        Console.WriteLine("together, and verify by re-deriving rather than reversing anything.");

    }
}