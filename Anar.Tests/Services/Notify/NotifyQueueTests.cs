namespace Anar.Tests.Services.Notify;

using Anar.Services.Notify;
public class NotifyQueueTests
{
    [Fact]
    public void Enqueue_AddsItems()
    {
        // Arrange
        var queue = new NotifyQueue();

        // Act
        queue.Enqueue(new SimpleAlert("test message"));
        queue.Enqueue(new ThumbprintAlert("expected", "actual"));

        Alert? alert = null;

        // Assert
        Assert.True(queue.TryDequeue(out alert));
        Assert.IsType<SimpleAlert>(alert);

        Assert.True(queue.TryDequeue(out alert));
        Assert.IsType<ThumbprintAlert>(alert);

        Assert.False(queue.TryDequeue(out alert));
    }
}