namespace NoP77svk.OraLobUnload.DataReaders;

using System;
using System.Collections.Generic;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public interface IDataMultiReader : IDisposable
{
    IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex);

    protected static async IAsyncEnumerable<DataMultiReaderRow> FetchDataFromReaderAsync(int lobNameColumnIndexBase1, int lobContentsColumnIndexBase1, OracleDataReader reader)
    {
        int leastDatasetColumnCountNeeded = Math.Max(lobNameColumnIndexBase1, lobContentsColumnIndexBase1);
        if (reader.FieldCount < leastDatasetColumnCountNeeded)
        {
            throw new InvalidDataException($"Dataset field count is {reader.FieldCount}, should be at least {leastDatasetColumnCountNeeded}");
        }

        string fileNameColumnTypeName = reader.GetFieldType(lobNameColumnIndexBase1 - 1).Name;
        if (fileNameColumnTypeName != "String")
        {
            throw new InvalidDataException($"Supposed file name column #{lobNameColumnIndexBase1} is of type \"{fileNameColumnTypeName}\", but \"string\" expected");
        }

        Type providerSpecificLobColumnType = reader.GetProviderSpecificFieldType(lobContentsColumnIndexBase1 - 1);
        Func<OracleDataReader, Stream?> getLobContentsFunc = providerSpecificLobColumnType switch
        {
            Type t when t == typeof(OracleClob) => r => r.GetOracleClob(lobContentsColumnIndexBase1 - 1),
            Type t when t == typeof(OracleBlob) => r => r.GetOracleBlob(lobContentsColumnIndexBase1 - 1),
            Type t when t == typeof(OracleBFile) => r => r.GetOracleBFile(lobContentsColumnIndexBase1 - 1),
            _ => throw new InvalidDataException($"Unsupported LOB column type: {providerSpecificLobColumnType.FullName}")
        };

        while (await reader.ReadAsync())
        {
            string lobName = reader.GetString(lobNameColumnIndexBase1 - 1);
            Stream? lobContentsStream = getLobContentsFunc(reader);
            yield return new DataMultiReaderRow(lobName, lobContentsStream);
        }
    }
}

#pragma warning disable SA1313
public record DataMultiReaderRow(string LobName, Stream? LobContents);
