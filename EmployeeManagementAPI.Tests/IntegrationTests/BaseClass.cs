using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public static class Payloads
{
    public static object Register(string username) => new
    {
        Name = "Test User",
        Designation = "Developer",
        Address = "123 Test St",
        Department = "Engineering",
        JoiningDate = new DateTime(2023, 1, 1),
        Skillset = "C#",
        Username = username,
        Password = "Password123!",
        CreatedBy = "Self"
    };

    public static object Login(string username) => new
    {
        Username = username,
        Password = "Password123!"
    };
}

public abstract class IntegrationTestBase : IClassFixture<EmployeeApiFactory>
{
    protected readonly HttpClient Client;
    protected readonly EmployeeApiFactory Factory;

    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    protected IntegrationTestBase(EmployeeApiFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    protected async Task<int> RegisterAndLoginAsync(string username)
    {
        var reg = await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register(username));
        reg.StatusCode.Should().Be(HttpStatusCode.OK, $"registration of '{username}' should succeed");

        var login = await Client.PostAsJsonAsync("/api/employee/ef/login", Payloads.Login(username));
        login.StatusCode.Should().Be(HttpStatusCode.OK, $"login of '{username}' should succeed");

        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.GetProperty("token").GetString()!;
        var id = body.GetProperty("employeeID").GetInt32();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return id;
    }

    protected void ClearAuth() =>
        Client.DefaultRequestHeaders.Authorization = null;

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r) =>
        JsonSerializer.Deserialize<JsonElement>(
            await r.Content.ReadAsStringAsync(), _json);
}