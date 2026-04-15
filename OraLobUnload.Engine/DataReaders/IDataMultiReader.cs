namespace NoP77svk.OraLobUnload.Engine.DataReaders;

using System;
using System.Collections.Generic;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public interface IDataMultiReader : IDisposable
{
    IAsyncEnumerable<DataMultiReaderRow> GetDataAsync(int fieldNameIndex, int fieldValueIndex);

    protected static async IAsyncEnumerable<DataMultiReaderRow> FetchDataFromReaderAsync(int lobNameColumnIndex, int lobContentsColumnIndex, OracleDataReader reader)
    {
        int leastDatasetColumnCountNeeded = Math.Max(lobNameColumnIndex, lobContentsColumnIndex) + 1;
        if (reader.FieldCount < leastDatasetColumnCountNeeded)
        {
            throw new InvalidDataException($"Dataset field count is {reader.FieldCount}, should be at least {leastDatasetColumnCountNeeded}");
        }

        string fileNameColumnTypeName = reader.GetFieldType(lobNameColumnIndex).Name;
        if (fileNameColumnTypeName != "String")
        {
            throw new InvalidDataException($"Supposed file name column #{lobNameColumnIndex} is of type \"{fileNameColumnTypeName}\", but \"string\" expected");
        }

        Type providerSpecificLobColumnType = reader.GetProviderSpecificFieldType(lobContentsColumnIndex);
        Func<OracleDataReader, Stream?> getLobContentsFunc = providerSpecificLobColumnType switch
        {
            Type t when t == typeof(OracleClob) => r => r.GetOracleClob(lobContentsColumnIndex),
            Type t when t == typeof(OracleBlob) => r => r.GetOracleBlob(lobContentsColumnIndex),
            Type t when t == typeof(OracleBFile) => r => r.GetOracleBFile(lobContentsColumnIndex),
            _ => throw new InvalidDataException($"Unsupported LOB column type: {providerSpecificLobColumnType.FullName}")
        };

        while (await reader.ReadAsync())
        {
            string lobName = reader.GetString(lobNameColumnIndex);
            Stream? lobContentsStream = getLobContentsFunc(reader);
            yield return new DataMultiReaderRow(lobName, lobContentsStream);
        }
    }
}

#pragma warning disable SA1313
public record DataMultiReaderRow(string LobName, Stream? LobContents);
