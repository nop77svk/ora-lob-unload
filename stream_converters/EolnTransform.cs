namespace NoP77svk.Lib.StreamProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    public class EolnTransform : ICryptoTransform
    {
        // reference: https://en.wikipedia.org/wiki/Newline#Representation
        public static string EolnSeqAsciiLf = "\u000a";
        public static string EolnSeqAsciiCr = "\u000d";
        public static string EolnSeqAsciiCrLf = "\u000d\u000a";
        public static string EolnSeqAsciiLfCr = "\u000a\u000a";
        public static string EolnSeqUnicodeNel = "\u0085";
        public static string EolnSeqUnicodeLineSep = "\u2028";
        public static string EolnSeqUnicodeParSep = "\u2029";
        public static string EolnSeqUnicodeVertTab = "\u000b";

        private static readonly string[] EolnsConsidered = {
            EolnSeqAsciiCrLf,
            EolnSeqAsciiLfCr,
            EolnSeqAsciiLf,
            EolnSeqAsciiCr,
            EolnSeqUnicodeNel,
            EolnSeqUnicodeLineSep,
            EolnSeqUnicodeParSep,
            EolnSeqUnicodeVertTab
        };

        private byte[] _targetEoln;
        private readonly byte[][] _rawEolnsConsidered;
        private readonly int _longestRawEolnConsidered;

        public Encoding StreamEncoding { get; }

        public string TargetEolnSequence
        {
            get { return StreamEncoding.GetString(_targetEoln); }
            set { _targetEoln = StreamEncoding.GetBytes(value); }
        }

        public EolnTransform(Encoding streamEncoding, string targetEolnSequence, int inputBufferSizeInChars = 262144)
        {
            StreamEncoding = streamEncoding;
            TargetEolnSequence = targetEolnSequence;

            _rawEolnsConsidered = new byte[EolnsConsidered.Length][];
            _longestRawEolnConsidered = 0;
            for (int i = 0; i < EolnsConsidered.Length; i++)
            {
                byte[] eolnSequenceRaw = StreamEncoding.GetBytes(EolnsConsidered[i]);
                _rawEolnsConsidered[i] = eolnSequenceRaw;
                if (eolnSequenceRaw.Length > _longestRawEolnConsidered)
                    _longestRawEolnConsidered = eolnSequenceRaw.Length;
            }

            InputBlockSize = StreamEncoding.GetMaxByteCount(inputBufferSizeInChars);
            OutputBlockSize = InputBlockSize * _longestRawEolnConsidered;
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

        protected int TransformBlockInternal(ReadOnlySpan<byte> input, Span<byte> output, int reservedBytesAtTheEnd = 0)
        {
            int bytesWritten = 0;
            int lastInputSpanOffset = 0;

            for (int inputSpanOffset = 0; inputSpanOffset < input.Length - reservedBytesAtTheEnd; inputSpanOffset++) // 2do! how many to subtract??
            {
                for (int rawEolnIx = 0; rawEolnIx < _rawEolnsConsidered.Length; inputSpanOffset++)
                {
                    if (input.Slice(inputSpanOffset, _rawEolnsConsidered[rawEolnIx].Length).SequenceEqual(_rawEolnsConsidered[rawEolnIx]))
                    {
                        int sliceLength = inputSpanOffset - lastInputSpanOffset;
                        input.Slice(lastInputSpanOffset, sliceLength).CopyTo(output.Slice(bytesWritten, sliceLength));
                        bytesWritten += sliceLength;

                        _targetEoln.CopyTo(output.Slice(bytesWritten, _targetEoln.Length));
                        bytesWritten += _targetEoln.Length;

                        inputSpanOffset += _rawEolnsConsidered[rawEolnIx].Length - 1;
                        break;
                    }
                }
            }

            return bytesWritten;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            AssertValidEolnsSupplied();
            int bytesWritten = 0;

            ReadOnlySpan<byte> inputSpan = inputBuffer[inputOffset..(inputOffset + inputCount)];
            int lastInputSpanOffset = 0;

            for (int inputSpanOffset = 0; inputSpanOffset < inputSpan.Length - _longestRawEolnConsidered + 1; inputSpanOffset++)
            {
                for (int rawEolnIx = 0; rawEolnIx < _rawEolnsConsidered.Length; inputSpanOffset++)
                {
                    if (inputSpan.Slice(inputSpanOffset, _rawEolnsConsidered[rawEolnIx].Length - 1).SequenceEqual(_rawEolnsConsidered[rawEolnIx]))
                    {
                        inputSpan[lastInputSpanOffset..inputSpanOffset].CopyTo(outputBuffer[(outputOffset + bytesWritten)..]);
                        bytesWritten += inputSpanOffset - lastInputSpanOffset;

                        _targetEoln.CopyTo(outputBuffer, outputOffset + bytesWritten);
                        bytesWritten += _targetEoln.Length;

                        inputSpanOffset += _rawEolnsConsidered[rawEolnIx].Length - 1;
                        break;
                    }
                }
            }

            return bytesWritten;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            AssertValidEolnsSupplied();

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
