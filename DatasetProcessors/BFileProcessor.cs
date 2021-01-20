namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class BFileProcessor : IStreamColumnProcessor
    {
        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleBFile(fieldIndex);
        }

        public long GetTrueLobLength(long reportedLength)
        {
            return reportedLength;
        }

        public string GetFormattedLobLength(long reportedLength)
        {
            return $"{GetTrueLobLength(reportedLength)} bytes long BLOB";
        }

        public void SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleBFile)
                throw new ArgumentException($"Must be OracleBFile, is {inLob.GetType().FullName}", nameof(inLob));

            inLob.CopyTo(outFile);
        }
    }
}
