namespace Ilnitsky.Polls.Tests.Smoke;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        SmokeTestFactory.DisposeInstance();
    }
}
