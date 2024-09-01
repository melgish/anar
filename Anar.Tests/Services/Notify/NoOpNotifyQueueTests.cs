namespace Anar.Tests.Services.Notify;

using Anar.Services.Notify;

public class NoOpNotifyQueueTests
{
    [Fact]
    public void Enqueue_DoesNotEnqueue()
    {
        // Arrange
        var queue = new NoOpNotifyQueue();

        // Act
        queue.Enqueue(new SimpleAlert("Test message"));
        queue.Enqueue(new ThumbprintAlert("expected", "actual"));

        // Assert
        Assert.False(queue.TryDequeue(out var alert));
        Assert.Null(alert);
    }
}