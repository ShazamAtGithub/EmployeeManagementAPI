using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class RegisterTests(EmployeeApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ValidRequest_Returns200WithEmployeeId()
    {
        var res = await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("reg_valid"));
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("employeeId").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("message").GetString().Should().Be("Registration successful");
    }

    [Fact]
    public async Task Register_DuplicateUsername_Returns400()
    {
        await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("reg_dup"));
        var res = await Client.PostAsJsonAsync("/api/employee/ef/register", Payloads.Register("reg_dup"));
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.GetProperty("message").GetString().Should().Contain("already exists");
    }

    [Fact]
    public async Task Register_InvalidBase64Image_Returns400()
    {
        var payload = new
        {
            Name = "Bad Image",
            Designation = "Dev",
            Address = "1 St",
            Department = "IT",
            JoiningDate = new DateTime(2023, 1, 1),
            Skillset = "C#",
            Username = "reg_badimg",
            Password = "Password123!",
            CreatedBy = "Self",
            Base64ProfileImage = "!!!not-valid-base64!!!"
        };

        var res = await Client.PostAsJsonAsync("/api/employee/ef/register", payload);
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.GetProperty("message").GetString().Should().Contain("valid Base64");
    }

    [Fact]
    public async Task Register_MissingRequiredFields_Returns400()
    {
        var res = await Client.PostAsJsonAsync("/api/employee/ef/register", new
        {
            Name = "No Creds"
        });

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}