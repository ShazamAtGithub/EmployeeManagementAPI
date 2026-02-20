using System.Net;
using FluentAssertions;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class GetEmployeeTests(EmployeeApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetEmployee_OwnProfile_Returns200WithData()
    {
        var id = await RegisterAndLoginAsync("get_own");
        var res = await Client.GetAsync($"/api/employee/ef/{id}");
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("employeeID").GetInt32().Should().Be(id);
    }

    [Fact]
    public async Task GetEmployee_OtherUserProfile_Returns404()
    {
        await RegisterAndLoginAsync("get_other");
        var res = await Client.GetAsync("/api/employee/ef/99999");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEmployee_NoToken_Returns401()
    {
        ClearAuth();
        var res = await Client.GetAsync("/api/employee/ef/1");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetEmployee_NonExistentId_Returns404()
    {
        await RegisterAndLoginAsync("get_notfound");
        var res = await Client.GetAsync("/api/employee/ef/88888");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}