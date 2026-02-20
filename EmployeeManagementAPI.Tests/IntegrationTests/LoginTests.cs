using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class LoginTests(EmployeeApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUserInfo()
    {
        await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("login_valid"));
        var res = await Client.PostAsJsonAsync("/api/employee/ef/login", Payloads.Login("login_valid"));
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("username").GetString().Should().Be("login_valid");
        body.GetProperty("role").GetString().Should().Be("Employee");
        body.GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("login_wrongpw"));
        var res = await Client.PostAsJsonAsync("/api/employee/ef/login", new
        {
            Username = "login_wrongpw",
            Password = "WrongPassword!"
        });
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        body.GetProperty("message").GetString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var res = await Client.PostAsJsonAsync("/api/employee/ef/login", Payloads.Login("login_ghost"));
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        body.GetProperty("message").GetString().Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task Login_UsernameIsCaseInsensitive_ReturnsToken()
    {
        await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("login_caseuser"));
        var res = await Client.PostAsJsonAsync("/api/employee/ef/login", new
        {
            Username = "LOGIN_CASEUSER",
            Password = "Password123!"
        });
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
}