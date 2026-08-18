using Application.Services;
using Domain.Entities;

namespace Web.EndpointExtensions;

public static class BankEndpointExtensions
{
    public static IEndpointRouteBuilder MapBankEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/bank")
            .WithTags("Bank");

        group.MapGet("/{id:int}", GetAccount);

        group.MapPost(
            "/{id:int}/deposit",
            Deposit);

        group.MapPost(
            "/{id:int}/withdraw",
            Withdraw);

        return app;
    }

    private static IResult GetAccount(
        IBankService service,
        int id)
    {
        var result = service.GetAccount(id);

        return result.Match(
            onSuccess: account =>
                Results.Ok(account),

            onFailure: Results.BadRequest);
    }

    private static IResult Deposit(
        IBankService service,
        int id,
        decimal amount)
    {
        var result = service
            .GetAccount(id)
            .Bind(account =>
                service.ValidateDeposit(account, amount))
            .Bind(account =>
                service.Deposit(account, amount));

        return result.Match(
            onSuccess: account =>
                Results.Ok(account),

            onFailure: Results.BadRequest);
    }

    private static IResult Withdraw(
        IBankService service,
        int id,
        decimal amount)
    {
        var result = service
            .GetAccount(id)
            .Bind(account =>
                service.ValidateWithdrawal(account, amount))
            .Bind(account =>
                service.Withdraw(account, amount));

        return result.Match(
            onSuccess: account =>
                Results.Ok(account),

            onFailure: Results.BadRequest);
    }
}