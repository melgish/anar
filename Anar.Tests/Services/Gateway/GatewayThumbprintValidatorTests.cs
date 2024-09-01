namespace Anar.Tests.Services.Gateway;

using Anar.Services;
using Anar.Services.Gateway;
using Anar.Services.Notify;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Text;

public class GatewayThumbprintValidatorTests
{
    private const string _testCertificate = """
    -----BEGIN CERTIFICATE-----
    MIIDhDCCAmygAwIBAgIBADANBgkqhkiG9w0BAQUFADBbMQswCQYDVQQGEwJVUzEQ
    MA4GA1UECAwHRmxvcmlkYTESMBAGA1UEBwwJTWVsYm91cm5lMRAwDgYDVQQKDAdU
    ZXN0aW5nMRQwEgYDVQQDDAtleGFtcGxlLmNvbTAeFw0yNDA5MDExMDQ2NDhaFw0y
    NTA5MDExMDQ2NDhaMFsxCzAJBgNVBAYTAlVTMRAwDgYDVQQIDAdGbG9yaWRhMRIw
    EAYDVQQHDAlNZWxib3VybmUxEDAOBgNVBAoMB1Rlc3RpbmcxFDASBgNVBAMMC2V4
    YW1wbGUuY29tMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAo11XvFMk
    WpXZ5qY1lr/tmyHD121FGt268aHdmLkN/3WTUj8IdjJre/9v1OY1JYAi91avo9pv
    l+baOErXTFG9meUpJi5NYVx1bx9X0sWqEPVeLi5UbIE0/U+bF+umj9jLq6A0FDlm
    4KddrIiohqB0tO4SwPs3xT6voGOXnq3dm/Va5WRugKpDlglwTvzNEx02MQE5sQZc
    8ySsrq0vr4lJ6CTvkInZstm5TuNkLqswio3S0nXcWwt2VG5lQnWH+G0JQmt8Xl8w
    yGPQP6DHrv7JT02smjw17lpEkoM+QCBDM9x5txng4vFtBADjxY+Syn8wY8n4hy85
    bLWxsgb8hjyPCwIDAQABo1MwUTAdBgNVHQ4EFgQUlS5stOeRYpGRyipFgllXqJxM
    dfQwHwYDVR0jBBgwFoAUlS5stOeRYpGRyipFgllXqJxMdfQwDwYDVR0TAQH/BAUw
    AwEB/zANBgkqhkiG9w0BAQUFAAOCAQEAeaTvaUDV9wo0qwDGQmB7jDXBR4OMgeUA
    DMRrV2tv/9hEBq288pvac/ZY8O78bSElDXI9PEqHPsTfhJQMf5A7wNLW24t/epnX
    SXj2w/T/reQXYjFV/gZ5PNVzOdL3MosLh0sAatn4idTT4fv9W5Fjsj8BQwufN5Ff
    zjOayS2Etlqoo/tXkFumAuVws4Vzx0MxVv3uFS7qCLYlR/OEVDwt4M3TD0BDhesV
    mdzhzl0LFDAAYA8/dv8wjVRG1K35GyU3zZDSk+Yqpm5kk9JzY1jhEag0+IUUSn8I
    ut9PDxCLZQS6CB7tGVOdRrN0giyxwqcRoXyaunBOd/Onb6TIf290Yw==
    -----END CERTIFICATE-----
    """;
    private const string _testThumbprint = "8982C28B9F31733C87BA6D5714D23277C1C8589A";
    private readonly FakeLogger<GatewayThumbprintValidator> _fakeLogger = new();
    private readonly NotifyQueue _notifyQueue = new();

    private GatewayThumbprintValidator CreateValidator(string thumbprint)
    {
        var options = Options.Create<GatewayOptions>(new() { Thumbprint = thumbprint });
        return new GatewayThumbprintValidator(
            options,
            _fakeLogger,
            _notifyQueue
        );
    }

    [Fact]
    public void ValidateThumbprint_WhenThereAreNoErrors_ReturnsTrue()
    {
        // Arrange
        var validator = CreateValidator("");

        // Act
        var result = validator.ValidateThumbprint(
            new(),
            null,
            null!,
            SslPolicyErrors.None
        );

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateThumbprint_WhenCertIsNull_ReturnsFalse()
    {
        // Arrange
        var validator = CreateValidator("");

        // Act
        var result = validator.ValidateThumbprint(
            new(),
            null,
            null!,
            SslPolicyErrors.RemoteCertificateNotAvailable
        );

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateThumbprint_WhenThumbprintMatches_ReturnsTrue()
    {
        // Arrange
        var validator = CreateValidator(_testThumbprint);

        // Act
        var result = validator.ValidateThumbprint(
            new(),
            new(Encoding.UTF8.GetBytes(_testCertificate)),
            null!,
            SslPolicyErrors.RemoteCertificateNameMismatch
        );

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateThumbprint_WhenThumbprintDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        var validator = CreateValidator("DE:AD:FA:CE");

        // Act
        var result = validator.ValidateThumbprint(
            new(),
            new(Encoding.UTF8.GetBytes(_testCertificate)),
            null!,
            SslPolicyErrors.RemoteCertificateNameMismatch
        );

        // Assert
        Assert.False(result);
        Assert.True(_notifyQueue.TryDequeue(out var alert));
        Assert.IsType<ThumbprintAlert>(alert);
        Assert.Equal(LogEvents.CertificateThumbprintMismatch, _fakeLogger.LatestRecord.Id);
    }
}