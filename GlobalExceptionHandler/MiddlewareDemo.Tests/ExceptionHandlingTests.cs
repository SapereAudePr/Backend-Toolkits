using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit.Abstractions;

namespace MiddlewareDemo.Tests;

public class ExceptionHandlingTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();


    [Fact]
    public async Task NotFoundEndpoint_MappedException_ReturnsNotFoundError()
    {
        HttpResponseMessage response = await _client.GetAsync("/not-found");

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        output.WriteLine(await response.Content.ReadAsStringAsync());

        Assert.NotNull(problem);
        Assert.Equal("Something was not found", problem.Detail);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task BadRequestEndpoint_UnMappedException_ReturnsInternalErrorError()
    {
        HttpResponseMessage response = await _client.GetAsync("/bad-request");

        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        output.WriteLine(await response.Content.ReadAsStringAsync());

        Assert.NotNull(problem);
        Assert.Equal("An unexpected error happened. Please try again.", problem.Detail);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.DoesNotContain("Something was invalid", problem.Detail);
    }

    [Fact]
    public async Task SuccessEndpoint_NotAnException_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/success");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}