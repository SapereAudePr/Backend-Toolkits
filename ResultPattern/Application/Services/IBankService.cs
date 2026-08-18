using Application.Common;
using Domain.Entities;

namespace Application.Services;

public interface IBankService
{
    Result<Account> GetAccount(int id);
    Result<Account> ValidateDeposit(Account account, decimal amount);
    Result<Account> Deposit(Account account, decimal amount);
    Result<Account> ValidateWithdrawal(Account account, decimal amount);
    Result<Account> Withdraw(Account account, decimal amount);
}