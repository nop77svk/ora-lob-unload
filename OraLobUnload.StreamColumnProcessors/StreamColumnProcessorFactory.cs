namespace NoP77svk.OraLobUnload.StreamColumnProcessors;
using System;
using System.IO;
using System.Text;

public static class StreamColumnProcessorFactory
{
    public static IStreamColumnProcessor CreateStreamColumnProcessor(Type columnType, string columnDescription, Encoding charColumnOutputEncoding)
    {
        return columnType.Name switch
        {
            "OracleClob" => new ClobProcessor(charColumnOutputEncoding),
            "OracleBlob" => new BlobProcessor(),
            "OracleBFile" => new BFileProcessor(),
            _ => throw new InvalidDataException($"Supposed LOB column {columnDescription} is of type \"{columnType.Name}\", but CLOB, BLOB or BFILE expected")
        };
    }
}
