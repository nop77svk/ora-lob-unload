namespace SK.NoP77svk.OraLobUnload.StreamColumnProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class BFileProcessor : IStreamColumnProcessor
    {
        private OracleBFile? _lobStream;

        public Stream ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            _lobStream = dataReader.GetOracleBFile(fieldIndex);
            if (!_lobStream.IsOpen)
                _lobStream.OpenFile();
            return _lobStream;
        }

        public long GetTrueLobLength(long reportedLength)
        {
            return reportedLength;
        }

        public string GetFormattedLobLength(long reportedLength)
        {
            return $"BFILE:{GetTrueLobLength(reportedLength)} bytes";
        }

        public void SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleBFile)
                throw new ArgumentException($"Must be OracleBFile, is {inLob.GetType().FullName}", nameof(inLob));

            inLob.CopyTo(outFile);
        }

        public void Dispose()
        {
            _lobStream?.Close();
            _lobStream?.Dispose();
        }
    }
}
