namespace OraLobUnload
{
    using System.IO;
    using Oracle.ManagedDataAccess.Types;

    public static class OracleManagedAccessFixes
    {
        private static readonly int UnicodeCharacterSizeInBytes = 2;

        /// <summary>
        /// A fix for OracleClob.CopyTo(Stream) badly messing up the copying.
        /// </summary>
        /// <param name="source">Source OracleClob stream.</param>
        /// <param name="target">Target Stream.</param>
        /// <param name="bufferSize">Internal buffer size for copying.</param>
        public static void CorrectlyCopyTo(this OracleClob source, Stream target, int bufferSize = 1048576)
        {
            var buf = new byte[bufferSize];
            int charsRead;
            int bytesRead;
            do
            {
                charsRead = source.Read(buf, 0, bufferSize); // note: OracleClob reports chars read, not bytes read!

                if (charsRead > 0)
                {
                    bytesRead = charsRead * UnicodeCharacterSizeInBytes;
                    if (bytesRead > 0)
                        target.Write(buf, 0, bytesRead);

                    source.Seek(bytesRead - charsRead, SeekOrigin.Current); // note: additional "shift" of stream origin due to OracleClob, even when reading bytes, moves the origin by number of chars read only
                }
            }
            while (charsRead > 0);
        }
    }
}
