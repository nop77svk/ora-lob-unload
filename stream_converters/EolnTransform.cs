namespace SK.NoP77svk.Lib.StreamProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public class EolnTransform : ICryptoTransform
    {
        private byte[] _targetEoln;
        private byte[] _blockTransformLeftover;

        public Encoding StreamEncoding { get; }

        // reference: https://en.wikipedia.org/wiki/Newline#Representation
        protected const string EolnSeqAsciiCrLf = "\u000d\u000a";
        protected const string EolnSeqAsciiLf = "\u000a";
        protected const string EolnSeqAsciiCr = "\u000d";
        protected const string EolnSeqAsciiLfCr = "\u000a\u000a";
        protected const string EolnSeqUnicodeNel = "\u0085";
        protected const string EolnSeqUnicodeVertTab = "\u000b";
        protected const string EolnSeqUnicodeLineSep = "\u2028";
        protected const string EolnSeqUnicodeParSep = "\u2029";
        protected string[] AsciiEolnSequencesPrioritized = {
            EolnSeqAsciiCrLf,
            EolnSeqAsciiLfCr,
            EolnSeqUnicodeLineSep,
            EolnSeqUnicodeNel,
            EolnSeqUnicodeParSep,
            EolnSeqUnicodeVertTab,
            EolnSeqAsciiLf,
            EolnSeqAsciiCr
        };

        public string TargetEolnSequence
        {
            get { return StreamEncoding.GetString(_targetEoln); }
            set { _targetEoln = StreamEncoding.GetBytes(value); }
        }

        public EolnTransform(Encoding streamEncoding, string targetEolnSequence, int inputBufferSizeInChars = 262144)
        {
            StreamEncoding = streamEncoding;
            TargetEolnSequence = targetEolnSequence;
            InputBlockSize = inputBufferSizeInChars;
        }

        public bool CanReuseTransform => false;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize { get; }

        public int OutputBlockSize { get; }

        public void Dispose()
        {
        }

        public EolnTransform ToWindows()
        {
            TargetEolnSequence = EolnSeqAsciiCrLf;
            return this;
        }

        public EolnTransform ToUnix()
        {
            TargetEolnSequence = EolnSeqAsciiLf;
            return this;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            AssertValidEolnsSupplied();
            DealWithPartialEolnsInInputBuffer(inputBuffer, inputCount, out byte[] inputBufferWithLeftovers, out byte[] newLeftover);

            throw new NotImplementedException();
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            AssertValidEolnsSupplied();
            DealWithPartialEolnsInInputBuffer(inputBuffer, inputCount, out byte[] inputBufferWithLeftovers, out byte[] newLeftover);

            throw new NotImplementedException();
        }

        private void AssertValidEolnsSupplied()
        {
            if (_targetEoln == null || _targetEoln.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(TargetEolnSequence), "NULL or empty target EOLN sequence supplied");
        }

        private void DealWithPartialEolnsInInputBuffer(byte[] inputBuffer, int inputCount, out byte[] inputBufferWithLeftovers, out byte[] newLeftover)
        {
            throw new NotImplementedException();
        }
    }
}
