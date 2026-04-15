namespace NoP77svk.OraLobUnload.Tests;

using System;
using System.IO;
using System.Text;
using NoP77svk.OraLobUnload.StreamColumnProcessors;
using Oracle.ManagedDataAccess.Types;
using Xunit;

/// <summary>
/// Tests for StreamColumnProcessorFactory to verify correct LOB processor selection.
/// </summary>
public class StreamColumnProcessorFactoryTests
{
    [Fact]
    public void CreateStreamColumnProcessor_WithOracleClobType_ReturnsClobProcessor()
    {
        // Arrange
        Type clobType = typeof(OracleClob);
        Encoding encoding = Encoding.UTF8;

        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor(clobType, encoding);

        // Assert
        Assert.IsType<ClobProcessor>(processor);
    }

    [Fact]
    public void CreateStreamColumnProcessor_WithOracleBlobType_ReturnsBlobProcessor()
    {
        // Arrange
        Type blobType = typeof(OracleBlob);

        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor(blobType, Encoding.UTF8);

        // Assert
        Assert.IsType<BlobProcessor>(processor);
    }

    [Fact]
    public void CreateStreamColumnProcessor_WithOracleBFileType_ReturnsBFileProcessor()
    {
        // Arrange
        Type bfileType = typeof(OracleBFile);

        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor(bfileType, Encoding.UTF8);

        // Assert
        Assert.IsType<BFileProcessor>(processor);
    }

    [Fact]
    public void CreateStreamColumnProcessor_WithInvalidType_ThrowsInvalidDataException()
    {
        // Arrange
        Type invalidType = typeof(string);

        // Act & Assert
        Assert.Throws<InvalidDataException>(() =>
            StreamColumnProcessorFactory.CreateStreamColumnProcessor(invalidType, Encoding.UTF8));
    }
}
