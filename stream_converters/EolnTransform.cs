namespace SK.NoP77svk.Lib.StreamProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public class EolnTransform : ICryptoTransform
    {
        private byte[] _sourceEoln;
        private byte[] _targetEoln;
        private byte[] _blockTransformLeftover;

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

        public EolnTransform(Encoding streamEncoding, int inputBufferSizeInChars = 262144)
        {
            StreamEncoding = streamEncoding;
            SourceEolnSequence = null;
            TargetEolnSequence = null;
            InputBlockSize = inputBufferSizeInChars;
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
            AssertValidEolnsSupplied();

            byte[] inputBufferWithLeftovers;
            byte[] newLeftover;
            DealWithPartialEolnsInInputBuffer(inputBuffer, inputCount, out inputBufferWithLeftovers, out newLeftover);

            ReadOnlySpan<byte> sourceEolnMem = _sourceEoln.AsSpan();
            ReadOnlySpan<byte> inputBufferMem = inputBufferWithLeftovers.AsSpan();
            int sourceOffset = 0;
            List<int> inputBufferEolnOffsets = new List<int>();
            while (sourceOffset > 0)
            {
                int eolnOffset = inputBufferMem.Slice(sourceOffset).IndexOf(sourceEolnMem);
                if (eolnOffset >= 0)
                    inputBufferEolnOffsets.Add(eolnOffset);
                sourceOffset = eolnOffset + sourceEolnMem.Length;
            }
            int outputBufferLength = inputBufferWithLeftovers.Length
                - _sourceEoln.Length * inputBufferEolnOffsets.Count
                + _targetEoln.Length * inputBufferEolnOffsets.Count;
            outputBuffer = new byte[outputBufferLength + outputOffset];

            _blockTransformLeftover = newLeftover;
            throw new NotImplementedException();
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            AssertValidEolnsSupplied();

            throw new NotImplementedException();
        }

        private void AssertValidEolnsSupplied()
        {
            if (_sourceEoln == null || _sourceEoln.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(SourceEolnSequence), "NULL or empty source EOLN sequence supplied");

            if (_targetEoln == null || _targetEoln.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(TargetEolnSequence), "NULL or empty target EOLN sequence supplied");

            if (_sourceEoln.Length > InputBlockSize)
                throw new ArgumentOutOfRangeException(nameof(InputBlockSize), "Source EOLN sequence is longer than input block size");
        }

        private void DealWithPartialEolnsInInputBuffer(byte[] inputBuffer, int inputCount, out byte[] inputBufferWithLeftovers, out byte[] newLeftover)
        {
            int totalBytesToTransform = _blockTransformLeftover?.Length ?? 0;
            int newLeftoverLength;

            if (inputCount > _sourceEoln.Length)
            {
                int partialDetectedSourceEolnLength = _sourceEoln.Length;
                while (partialDetectedSourceEolnLength > 0)
                {
                    partialDetectedSourceEolnLength--;
                    // compare [partialDetectedSourceEolnLength] trailing bytes of inputBuffer with [partialDetectedSourceEolnLength] leading bytes of _sourceEoln
                    if (inputBuffer.AsSpan(inputBuffer.Length - partialDetectedSourceEolnLength).SequenceEqual(_sourceEoln.AsSpan(0, partialDetectedSourceEolnLength - 1)))
                        break;
                }
                int inputCountWoPartialEoln = inputCount - partialDetectedSourceEolnLength;
                totalBytesToTransform += inputCountWoPartialEoln;

                newLeftoverLength = partialDetectedSourceEolnLength;
            }
            else
            {
                totalBytesToTransform += inputCount;
                newLeftoverLength = 0;
            }

            if (_blockTransformLeftover?.Length > 0)
            {
                inputBufferWithLeftovers = new byte[totalBytesToTransform];
                _blockTransformLeftover.CopyTo(inputBufferWithLeftovers, 0);

                totalBytesToTransform -= _blockTransformLeftover.Length;
                inputBuffer.AsSpan(0, totalBytesToTransform).CopyTo(inputBufferWithLeftovers.AsSpan(_blockTransformLeftover.Length));
            }
            else
            {
                inputBufferWithLeftovers = inputBuffer;
            }

            if (newLeftoverLength > 0)
                newLeftover = inputBuffer[^newLeftoverLength..];
            else
                newLeftover = null;
        }
    }
}
