namespace SK.NoP77svk.OraLobUnload
{
    using System.IO;
    using System.Text;
    using Oracle.ManagedDataAccess.Types;

    public static class OracleManagedAccessFixes
    {
        /// <summary>
        /// A fix for OracleClob.CopyTo(Stream) badly messing up the copying.
        /// </summary>
        /// <param name="source">Source OracleClob stream.</param>
        /// <param name="target">Target Stream.</param>
        /// <param name="bufferSize">Internal buffer size for copying.</param>
        public static void CorrectlyCopyTo(this OracleClob source, Stream target, int bufferSize = 262144)
        {
            var buf = new byte[bufferSize * UnicodeEncoding.CharSize];
            int charsRead;
            int bytesRead;
            do
            {
                charsRead = source.Read(buf, 0, bufferSize); // note: OracleClob reports chars read, not bytes read!

                if (charsRead > 0)
                {
                    bytesRead = charsRead * UnicodeEncoding.CharSize;
                    if (bytesRead > 0)
                        target.Write(buf, 0, bytesRead);

                    source.Seek(bytesRead - charsRead, SeekOrigin.Current); // note: additional "shift" of stream origin due to OracleClob, even when reading bytes, moves the origin by number of chars read only
                }
            }
            while (charsRead > 0);
        }
    }
}
