namespace OraLobUnload.StreamColumnProcessors
{
    using System.IO;
    using Oracle.ManagedDataAccess.Client;

    internal interface IStreamColumnProcessor
    {
        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex);

        public long GetTrueLobLength(long reportedLength);

        public string GetFormattedLobLength(long reportedLength);

        public void SaveLobToStream(Stream inLob, Stream outFile);
    }
}
