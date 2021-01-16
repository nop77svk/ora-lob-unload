namespace OraLobUnload.DatasetProcessors
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal class BlobProcessor : IDataReaderToStream
    {
        Stream IDataReaderToStream.ReadLob(OracleDataReader dataReader, int fieldIndex)
        {
            return dataReader.GetOracleBlob(fieldIndex);
        }

        long IDataReaderToStream.GetTrueLobLength(long reportedLength)
        {
            return reportedLength;
        }

        void IDataReaderToStream.SaveLobToStream(Stream inLob, Stream outFile)
        {
            if (inLob is not OracleBlob)
                throw new ArgumentException($"Must be OracleBlob, is {inLob.GetType().FullName}", nameof(inLob));

            inLob.CopyTo(outFile);
        }
    }
}
