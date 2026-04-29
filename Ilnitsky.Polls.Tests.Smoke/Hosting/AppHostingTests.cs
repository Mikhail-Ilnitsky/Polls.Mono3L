using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

namespace Ilnitsky.Polls.Tests.Smoke.Hosting;

public class AppHostingTests
{
    [Test]
    public async Task AppHealthCheckEndpoint_ReturnsHealthy()
    {
        // Arrange
        var httpClient = SmokeTestFactory.GetInstance().CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeOneOf("Healthy", "Degraded");
    }

    [Test]
    public async Task AppLivenessEndpoint_ReturnsHealthy()
    {
        // Arrange
        var httpClient = SmokeTestFactory.GetInstance().CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Test]
    public async Task AppReadinessEndpoint_ReturnsOk()
    {
        // Arrange
        var httpClient = SmokeTestFactory.GetInstance().CreateClient();

        // Act
        var response = await httpClient.GetAsync("/health/ready");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task AppMetricsEndpoint_ReturnsData()
    {
        // Arrange
        var httpClient = SmokeTestFactory.GetInstance().CreateClient();

        // Act
        var response = await httpClient.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty(); // Проверяем, что метрики генерируются
    }
}
