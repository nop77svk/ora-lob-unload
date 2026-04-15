namespace NoP77svk.OraLobUnload.Tests;

using System;
using System.IO;
using NoP77svk.OraLobUnload.StreamColumnProcessors;
using Oracle.ManagedDataAccess.Types;
using Xunit;

/// <summary>
/// Unit tests for BlobProcessor functionality.
/// </summary>
public class BlobProcessorTests
{
    [Fact]
    public void GetFormattedLobLength_ForBlob_ReturnsCorrectFormat()
    {
        // Arrange
        var processor = new BlobProcessor();
        long blobLength = 1024 * 100; // 100 KB

        // Act
        string formatted = processor.GetFormattedLobLength(blobLength);

        // Assert
        Assert.Equal($"BLOB:{blobLength} bytes", formatted);
    }

    [Fact]
    public void GetTrueLobLength_ForBlob_ReturnsSameValue()
    {
        // Arrange
        var processor = new BlobProcessor();
        long reportedLength = 2048;

        // Act
        long trueLength = processor.GetTrueLobLength(reportedLength);

        // Assert
        Assert.Equal(reportedLength, trueLength);
    }

    [Fact]
    public void SaveLobToStream_WithNonOracleBlobStream_ThrowsArgumentException()
    {
        // Arrange
        var processor = new BlobProcessor();
        using var invalidStream = new MemoryStream(new byte[] { 1, 2, 3 });
        using var outputStream = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            processor.SaveLobToStream(invalidStream, outputStream));
    }
}
