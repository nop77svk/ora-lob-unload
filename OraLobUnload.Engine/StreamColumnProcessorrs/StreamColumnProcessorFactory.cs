namespace NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using System;
using System.IO;
using System.Text;

using Oracle.ManagedDataAccess.Types;

public static class StreamColumnProcessorFactory
{
    public static IStreamColumnProcessor CreateStreamColumnProcessor(Type? columnType, Encoding charColumnOutputEncoding)
    {
        return columnType?.Name switch
        {
            nameof(OracleClob) => new ClobProcessor(charColumnOutputEncoding),
            nameof(OracleBlob) => new BlobProcessor(),
            nameof(OracleBFile) => new BFileProcessor(),
            _ => throw new InvalidDataException($"Supposed LOB column is of type \"{columnType?.Name}\", but CLOB, BLOB or BFILE expected")
        };
    }

    public static IStreamColumnProcessor CreateStreamColumnProcessor<T>(Encoding charColumnOutputEncoding)
        => CreateStreamColumnProcessor(typeof(T), charColumnOutputEncoding);
}
