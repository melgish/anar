using Anar.Tests.Services;

public class TestTimeProvider() : TimeProvider
{
    public DateTimeOffset Now { get; set; } = System.GetUtcNow();
    public override DateTimeOffset GetUtcNow() => Now;

    public void SetTwoDaysAgo()
        => Now = System.GetUtcNow().Subtract(TimeSpan.FromDays(2));

    public void SetNow()
        => Now = System.GetUtcNow();
}