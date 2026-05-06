using System.Net;

using FluentAssertions;

namespace Ilnitsky.Polls.Tests.Integration.NUnit.Hosting;

public class AppHealthTests
{
    [Test]
    public async Task AppHealthCheckEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await GlobalTestsSetup.HttpClient.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().BeOneOf("Healthy", "Degraded");
    }

    [Test]
    public async Task AppLivenessEndpoint_ReturnsHealthy()
    {
        // Act
        var response = await GlobalTestsSetup.HttpClient.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Be("Healthy");
    }

    [Test]
    public async Task AppReadinessEndpoint_ReturnsOk()
    {
        // Act
        var response = await GlobalTestsSetup.HttpClient.GetAsync("/health/ready");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task AppMetricsEndpoint_ReturnsData()
    {
        // Act
        var response = await GlobalTestsSetup.HttpClient.GetAsync("/metrics");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Проверяем наличие стандартных имен метрик prometheus-net
        content.Should().Contain("http_requests_received_total");
        content.Should().Contain("http_request_duration_seconds");

        // Проверяем наличие имен метрик MariaDB (MySqlConnector)
        content.Should().Contain("mysqlconnector_db_client_connections_max");
    }
}
