namespace OraLobUnload
{
    using System;
    using System.IO;
    using System.Text;
    using Oracle.ManagedDataAccess.Types;

    public static class OracleManagedAccessFixes
    {
        /// <summary>
        /// OracleClob.CopyTo(some binary stream) simply messing up the output.
        /// OracleClob.Read(byte[] buf,...) reads "count" bytes, but reports "count" characters, i.e. half the bytes.
        /// OracleClob.Read(byte[] bug,...) reads "count" bytes, but shifts the stream origin only by half the bytes further.
        /// </summary>
        /// <param name="source">source OracleClob stream.</param>
        /// <param name="target">target Stream.</param>
        /// <param name="bufferSize">internal buffer size for copying.</param>
        public static void CorrectlyCopyTo(this OracleClob source, Stream target, int bufferSize = 1048576)
        {
            var buf = new byte[bufferSize];
            int charsRead;
            int bytesRead;
            do
            {
                charsRead = source.Read(buf, 0, bufferSize); // note: OracleClob reports chars read, not bytes read!
                source.Seek(charsRead, SeekOrigin.Current); // note: OracleClob, even when reading bytes, moves the "current origin" by number of chars read only

                bytesRead = charsRead * 2;
                if (bytesRead > 0)
                    target.Write(buf, 0, bytesRead);
            }
            while (bytesRead >= bufferSize);
        }
    }
}
