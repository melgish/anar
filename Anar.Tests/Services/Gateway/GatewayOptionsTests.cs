namespace Anar.Services.Gateway;

public class GatewayClientTests
{
    [Fact]
    public void ThumbprintMutator_ReplacesNonHexDigits()
    {
        // Arrange
        var options = new GatewayOptions
        {
            Thumbprint = "A1:b2:C3-d4-E5-f6-Gg-78-90",
        };

        // Act
        var thumbprint = options.Thumbprint;

        // Assert
        Assert.Equal("A1b2C3d4E5f67890", thumbprint);
    }
}