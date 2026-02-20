using EmployeeManagementAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EmployeeManagementAPI.Tests.Integration;

public class EmployeeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseSqlite(_connection));
        });

        builder.UseSetting("Jwt:Key", "95408952bb03db93db0f13d8fa3b2482");
        builder.UseSetting("Jwt:Issuer", "EmployeeManagementAPI");
        builder.UseSetting("Jwt:Audience", "EmployeeManagementAppUsers");
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await base.DisposeAsync();
    }
}