using Application.Common;
using Domain.Entities;

namespace Application.Services;

public class BankService : IBankService
{
    private readonly List<Account> _accounts =
    [
        new("John", 1),
        new("Raven", 2)
    ];

    public Result<Account> GetAccount(int id)
    {
        var account = _accounts.FirstOrDefault(x => x.Id == id);

        if (account is null)
        {
            return Result<Account>.Failure(
            [
                new ErrorMessage(
                    "id",
                    $"Account with id {id} was not found.")
            ]);
        }

        return account;
    }

    public Result<Account> ValidateDeposit(
        Account account,
        decimal amount)
    {
        if (amount <= 0)
        {
            return Result<Account>.Failure(
            [
                new ErrorMessage(
                    "amount",
                    "Deposit amount must be greater than zero.")
            ]);
        }

        return account;
    }

    public Result<Account> Deposit(
        Account account,
        decimal amount)
    {
        account.Deposit(amount);

        return account;
    }

    public Result<Account> ValidateWithdrawal(
        Account account,
        decimal amount)
    {
        if (amount <= 0)
        {
            return Result<Account>.Failure(
            [
                new ErrorMessage(
                    "amount",
                    "Withdrawal amount must be greater than zero.")
            ]);
        }

        if (account.Balance < amount)
        {
            return Result<Account>.Failure(
            [
                new ErrorMessage(
                    "amount",
                    "Insufficient balance.")
            ]);
        }

        return account;
    }

    public Result<Account> Withdraw(
        Account account,
        decimal amount)
    {
        account.Withdraw(amount);

        return account;
    }
}