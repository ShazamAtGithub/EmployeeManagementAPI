using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class ProfileImageTests(EmployeeApiFactory factory) : IntegrationTestBase(factory)
{
    // 1x1 transparent PNG as Base64
    private const string ValidBase64Image =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

    [Fact]
    public async Task UpdateProfileImage_ValidBase64_Returns200()
    {
        var id = await RegisterAndLoginAsync("img_update");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id}/image", new
        {
            Base64Image = ValidBase64Image,
            ModifiedBy = "img_update"
        });
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("message").GetString().Should().Be("Image updated successfully");
    }

    [Fact]
    public async Task GetProfileImage_AfterUpload_ReturnsBase64()
    {
        var id = await RegisterAndLoginAsync("img_get");
        await Client.PutAsJsonAsync($"/api/employee/ef/{id}/image", new
        {
            Base64Image = ValidBase64Image,
            ModifiedBy = "img_get"
        });

        var res = await Client.GetAsync($"/api/employee/ef/{id}/image");
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        body.GetProperty("image").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProfileImage_NoImageUploaded_Returns404()
    {
        var id = await RegisterAndLoginAsync("img_none");
        var res = await Client.GetAsync($"/api/employee/ef/{id}/image");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProfileImage_InvalidBase64_Returns400()
    {
        var id = await RegisterAndLoginAsync("img_badb64");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id}/image", new
        {
            Base64Image = "!!!invalid!!!",
            ModifiedBy = "img_badb64"
        });
        var body = await ReadJsonAsync(res);

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        body.GetProperty("message").GetString().Should().Contain("valid Base64");
    }

    [Fact]
    public async Task UpdateProfileImage_DifferentUserId_Returns403()
    {
        var id = await RegisterAndLoginAsync("img_forbid");
        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id + 999}/image", new
        {
            Base64Image = ValidBase64Image,
            ModifiedBy = "img_forbid"
        });

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateProfileImage_NullImage_ClearsExistingImage()
    {
        var id = await RegisterAndLoginAsync("img_clear");

        await Client.PutAsJsonAsync($"/api/employee/ef/{id}/image", new
        {
            Base64Image = ValidBase64Image,
            ModifiedBy = "img_clear"
        });

        var res = await Client.PutAsJsonAsync($"/api/employee/ef/{id}/image", new
        {
            Base64Image = (string?)null,
            ModifiedBy = "img_clear"
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var getRes = await Client.GetAsync($"/api/employee/ef/{id}/image");
        getRes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
