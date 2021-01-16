namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class BFileProcessor : IDataReaderToStream
    {
        Stream IDataReaderToStream.ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleBFile(fieldIndex);
        }

        long IDataReaderToStream.GetTrueLobLength(long reportedLength)
        {
            return reportedLength;
        }

        void IDataReaderToStream.SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleBFile)
                throw new ArgumentException($"Must be OracleBFile, is {inLob.GetType().FullName}", nameof(inLob));

            inLob.CopyTo(outFile);
        }
    }
}
