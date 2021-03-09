namespace SK.NoP77svk.OraLobUnload.StreamColumnProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class BlobProcessor : IStreamColumnProcessor
    {
        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleBlob(fieldIndex);
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
            if (inLob is not OracleBlob)
                throw new ArgumentException($"Must be OracleBlob, is {inLob.GetType().FullName}", nameof(inLob));

            inLob.CopyTo(outFile);
        }
    }
}
