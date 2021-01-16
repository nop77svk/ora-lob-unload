namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal interface IDataReaderToStream
    {
        internal Stream ReadLob(OracleDataReader dataReader, int fieldIndex);

        internal long GetTrueLobLength(long reportedLength);

        internal void SaveLobToStream(Stream inLob, Stream outFile);
    }
}
