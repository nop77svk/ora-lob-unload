namespace NoP77svk.lib.StreamProcessors
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class EolnTransform : ICryptoTransform
    {
        private byte[] _sourceEoln;
        private byte[] _targetEoln;

        public Encoding StreamEncoding { get; }

        protected const string WindowsEolnSequence = "\000d\000a";
        protected const string UnixEolnSequence = "\000a";

        protected byte[] WindowsEolnByteSequence => StreamEncoding.GetBytes(WindowsEolnSequence);
        protected byte[] UnixEolnBytesSequence => StreamEncoding.GetBytes(UnixEolnSequence);

        public string SourceEolnSequence
        {
            get { return StreamEncoding.GetString(_sourceEoln); }
            set { _sourceEoln = StreamEncoding.GetBytes(value); }
        }

        public string TargetEolnSequence
        {
            get { return StreamEncoding.GetString(_targetEoln); }
            set { _targetEoln = StreamEncoding.GetBytes(value); }
        }

        public EolnTransform(Encoding streamEncoding)
        {
            StreamEncoding = streamEncoding;
            SourceEolnSequence = null;
            TargetEolnSequence = null;
        }

        public bool CanReuseTransform => false;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize { get; }

        public int OutputBlockSize { get; }

        public void Dispose()
        {
        }

        public EolnTransform FromWindows()
        {
            SourceEolnSequence = WindowsEolnSequence;
            return this;
        }

        public EolnTransform FromUnix()
        {
            SourceEolnSequence = UnixEolnSequence;
            return this;
        }

        public EolnTransform ToWindows()
        {
            TargetEolnSequence = WindowsEolnSequence;
            return this;
        }

        public EolnTransform ToUnix()
        {
            TargetEolnSequence = UnixEolnSequence;
            return this;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            throw new NotImplementedException();
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            throw new NotImplementedException();
        }
    }
}
