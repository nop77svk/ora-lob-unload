namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal interface IDataReaderToStream
    {
        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex);

        public long GetTrueLobLength(long reportedLength);

        public string GetFormattedLobLength(long reportedLength);

        public void SaveLobToStream(Stream inLob, Stream outFile);
    }
}
