using Application.Services;
using Domain.Entities;

namespace Tests;

public class ResultPatternTests
{
    private readonly BankService _service;

    public ResultPatternTests()
    {
        _service = new BankService();
    }

    [Fact]
    public void GetAccount_WhenAccountExists_ReturnsSuccess()
    {
        // Act
        var result = _service.GetAccount(1);

        // Assert
        var account = result.Match(
            onSuccess: account => account,
            onFailure: _ => null);

        Assert.NotNull(account);
        Assert.Equal(1, account.Id);
        Assert.Equal("John", account.OwnerName);
    }

    [Fact]
    public void GetAccount_WhenAccountDoesNotExist_ReturnsFailure()
    {
        // Act
        var result = _service.GetAccount(999);

        // Assert
        var message = result.Match(
            onSuccess: _ => null,
            onFailure: errors => errors.First().Message);

        Assert.Equal(
            "Account with id 999 was not found.",
            message);
    }

    [Fact]
    public void ValidateDeposit_WhenAmountIsPositive_ReturnsSuccess()
    {
        // Arrange
        var account = new Account("John", 1);

        // Act
        var result = _service.ValidateDeposit(account, 100);

        // Assert
        var resultAccount = result.Match(
            onSuccess: account => account,
            onFailure: _ => null);

        Assert.NotNull(resultAccount);
        Assert.Same(account, resultAccount);
    }

    [Fact]
    public void ValidateDeposit_WhenAmountIsZero_ReturnsFailure()
    {
        // Arrange
        var account = new Account("John", 1);

        // Act
        var result = _service.ValidateDeposit(account, 0);

        // Assert
        var error = result.Match(
            onSuccess: _ => null,
            onFailure: errors => errors.First());

        Assert.NotNull(error);
        Assert.Equal("amount", error.FieldName);
        Assert.Equal(
            "Deposit amount must be greater than zero.",
            error.Message);
    }

    [Fact]
    public void Deposit_IncreasesAccountBalance()
    {
        // Arrange
        var account = new Account("John", 1);

        // Act
        var result = _service.Deposit(account, 500);

        // Assert
        var resultAccount = result.Match(
            onSuccess: account => account,
            onFailure: _ => null);

        Assert.NotNull(resultAccount);
        Assert.Equal(500, resultAccount.Balance);
    }

    [Fact]
    public void ValidateWithdrawal_WhenAmountIsPositiveAndBalanceIsSufficient_ReturnsSuccess()
    {
        // Arrange
        var account = new Account("John", 1);
        account.Deposit(500);

        // Act
        var result = _service.ValidateWithdrawal(account, 300);

        // Assert
        var resultAccount = result.Match(
            onSuccess: account => account,
            onFailure: _ => null);

        Assert.NotNull(resultAccount);
        Assert.Same(account, resultAccount);
    }

    [Fact]
    public void ValidateWithdrawal_WhenAmountIsGreaterThanBalance_ReturnsFailure()
    {
        // Arrange
        var account = new Account("John", 1);
        account.Deposit(500);

        // Act
        var result = _service.ValidateWithdrawal(account, 600);

        // Assert
        var error = result.Match(
            onSuccess: _ => null,
            onFailure: errors => errors.First());

        Assert.NotNull(error);
        Assert.Equal("amount", error.FieldName);
        Assert.Equal("Insufficient balance.", error.Message);
    }

    [Fact]
    public void Withdraw_DecreasesAccountBalance()
    {
        // Arrange
        var account = new Account("John", 1);
        account.Deposit(500);

        // Act
        var result = _service.Withdraw(account, 200);

        // Assert
        var resultAccount = result.Match(
            onSuccess: account => account,
            onFailure: _ => null);

        Assert.NotNull(resultAccount);
        Assert.Equal(300, resultAccount.Balance);
    }
}