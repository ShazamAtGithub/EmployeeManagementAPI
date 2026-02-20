using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class UpdateEmployeeTests(EmployeeApiFactory factory) : IntegrationTestBase(factory)
{
    private static object UpdatePayload(string modifiedBy) => new
    {
        Name = "Updated Name",
        Designation = "Senior Developer",
        Address = "456 New St",
        Department = "Product",
        JoiningDate = new DateTime(2023, 1, 1),
        Skillset = "C#, Azure",
        ModifiedBy = modifiedBy
    };

    [Fact]
    public async Task UpdateEmployee_ValidRequest_Returns200()
    {
        var id = await RegisterAndLoginAsync("upd_valid");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id}", UpdatePayload("upd_valid"));
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("message").GetString().Should().Be("Profile updated successfully");
    }

    [Fact]
    public async Task UpdateEmployee_DifferentUserId_Returns403()
    {
        var id = await RegisterAndLoginAsync("upd_forbid");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id + 999}", UpdatePayload("upd_forbid"));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateEmployee_WrongModifiedBy_Returns404()
    {
        var id = await RegisterAndLoginAsync("upd_wrongmod");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id}", UpdatePayload("someone_else"));

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateEmployee_NoToken_Returns401()
    {
        ClearAuth();
        var res = await Client.PutAsJsonAsync("/api/employee/ef/1", UpdatePayload("nobody"));

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateEmployee_PersistsChanges_VerifiedByGet()
    {
        var id = await RegisterAndLoginAsync("upd_persist");
        await Client.PutAsJsonAsync($"/api/employee/ef/{id}", UpdatePayload("upd_persist"));

        var res = await Client.GetAsync($"/api/employee/ef/{id}");
        var body = await ReadJsonAsync(res);

        body.GetProperty("name").GetString().Should().Be("Updated Name");
        body.GetProperty("designation").GetString().Should().Be("Senior Developer");
        body.GetProperty("department").GetString().Should().Be("Product");
    }
}