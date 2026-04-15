namespace NoP77svk.OraLobUnload.Tests;

using System.Text;

using NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using Xunit;

/// <summary>
/// Unit tests for ClobProcessor functionality.
/// </summary>
public class ClobProcessorTests
{
    [Fact]
    public void GetFormattedLobLength_ForClob_ReturnsCorrectFormat()
    {
        // Arrange
        var processor = new ClobProcessor(Encoding.UTF8);
        long clobLength = 2048; // In bytes (Oracle Unicode = 2 bytes per char)

        // Act
        string formatted = processor.GetFormattedLobLength(clobLength);

        // Assert
        Assert.Equal("CLOB:1024 characters", formatted);
    }

    [Fact]
    public void GetTrueLobLength_ForClob_DividesLengthByTwo()
    {
        // Arrange
        var processor = new ClobProcessor(Encoding.UTF8);
        long reportedLength = 4096; // Reported in bytes

        // Act
        long trueLength = processor.GetTrueLobLength(reportedLength);

        // Assert
        Assert.Equal(2048, trueLength); // Should be half for Unicode
    }

    [Fact]
    public void Constructor_WithDifferentEncodings_InitializesSuccessfully()
    {
        // Arrange
        var encodings = new[] { Encoding.UTF8, Encoding.ASCII, Encoding.Unicode };

        // Act & Assert
        foreach (var encoding in encodings)
        {
            var processor = new ClobProcessor(encoding);
            Assert.NotNull(processor);
        }
    }
}
