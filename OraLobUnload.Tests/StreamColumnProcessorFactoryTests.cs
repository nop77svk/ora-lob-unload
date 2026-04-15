namespace NoP77svk.OraLobUnload.Tests;

using System;
using System.IO;
using System.Text;

using NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

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
        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor<OracleClob>(Encoding.UTF8);

        // Assert
        Assert.IsType<ClobProcessor>(processor);
    }

    [Fact]
    public void CreateStreamColumnProcessor_WithOracleBlobType_ReturnsBlobProcessor()
    {
        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor<OracleBlob>(Encoding.UTF8);

        // Assert
        Assert.IsType<BlobProcessor>(processor);
    }

    [Fact]
    public void CreateStreamColumnProcessor_WithOracleBFileType_ReturnsBFileProcessor()
    {
        // Act
        var processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor<OracleBFile>(Encoding.UTF8);

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
