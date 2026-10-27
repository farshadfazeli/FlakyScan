namespace FlakySample;

public sealed class FlakySampleTests
{
    [Fact]
    public void Stable_test_one()
    {
        Assert.True(true);
    }

    [Fact]
    public void Stable_test_two()
    {
        Assert.Equal(4, 2 + 2);
    }

    [Fact]
    public void Randomly_flaky_test()
    {
        Assert.True(Random.Shared.Next(2) == 0, "This sample intentionally fails sometimes.");
    }

    [Fact]
    public void Timing_flaky_test()
    {
        Thread.Sleep(25);
        Assert.True(DateTime.UtcNow.Millisecond % 2 == 0, "This sample intentionally depends on timing.");
    }
}
