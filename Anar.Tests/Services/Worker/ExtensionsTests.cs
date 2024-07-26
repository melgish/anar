using Anar.Services.Worker;

namespace Anar.Tests.Services.Worker;

public sealed class ExtensionsTests
{
    sealed record Point(int X, int Y);

    [Fact]
    public void When_WhenPredicateIsTrue_ShouldApplyAction()
    {
        // Arrange
        var point = new Point(0, 1);

        // Act
        var result = point.When(p => p.X == 0, p => new Point(p.X + 1, p.Y));

        // Assert
        Assert.Equal(new Point(1, 1), result);
    }

    [Fact]
    public void When_WhenPredicateIsFalse_ShouldNotApplyAction()
    {
        // Arrange
        // Arrange
        var point = new Point(0, 1);

        // Act
        var result = point.When(p => p.X != 0, p => new Point(p.X + 1, p.Y));

        // Assert
        Assert.Equal(new Point(0, 1), result);
    }
}
