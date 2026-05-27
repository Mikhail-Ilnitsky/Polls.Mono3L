using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using FluentAssertions;
using FluentAssertions.Execution;

namespace Ilnitsky.Polls.Tests.Integration.XUnit.Hosting;

[Collection("GlobalCollection")]
public class AppHealthTests(AppFixture fixture)
{
    private HttpClient HttpClient => fixture.HttpClient;

    [Fact]
    public async Task AppHealthCheckEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await HttpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().BeOneOf("Healthy", "Degraded");
        }
    }

    [Fact]
    public async Task AppLivenessEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await HttpClient.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("Healthy");
        }
    }

    [Fact]
    public async Task AppReadinessEndpoint_ReturnsOk()
    {
        // Act
        var response = await HttpClient.GetAsync("/health/ready");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task AppMetricsEndpoint_ReturnsData()
    {
        // Act
        var response = await HttpClient.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        using (new AssertionScope())
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Проверяем наличие стандартных имен метрик prometheus-net
            content.Should().Contain("http_requests_received_total");
            content.Should().Contain("http_request_duration_seconds");

            // Проверяем наличие имен метрик MariaDB (MySqlConnector)
            content.Should().Contain("mysqlconnector_db_client_connections_max");
        }
    }
}
