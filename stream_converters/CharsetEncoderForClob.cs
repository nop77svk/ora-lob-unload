namespace nop77svk.lib.StreamProcessors
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class CharsetEncoderForClob : ICryptoTransform
    {
        public CharsetEncoderForClob(UnicodeEncoding inputDecoder, Encoding outputEncoder, int inputBufferSizeInChars = 262144)
        {
            if (inputBufferSizeInChars <= 0)
                throw new ArgumentOutOfRangeException(nameof(inputBufferSizeInChars), $"Illegal input buffer size of \"{inputBufferSizeInChars}\" characters");

            InputBlockSize = inputBufferSizeInChars * 2;

            InputDecoder = inputDecoder;
            OutputEncoder = outputEncoder;
        }

        public bool CanReuseTransform => false;

        public bool CanTransformMultipleBlocks => true;

        public int InputBlockSize { get; }

        public int OutputBlockSize => InputBlockSize / InputDecoder.GetMaxByteCount(1) * OutputEncoder.GetMaxByteCount(1);

        internal UnicodeEncoding InputDecoder { get; }

        internal Encoding OutputEncoder { get; }

        public void Dispose()
        {
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            string decodedInput = InputDecoder.GetString(inputBuffer, inputOffset, inputCount);
            byte[] output = OutputEncoder.GetBytes(decodedInput);
            output.CopyTo(outputBuffer, outputOffset);
            return output.Length;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            string decodedInput = InputDecoder.GetString(inputBuffer, inputOffset, inputCount);
            byte[] output = OutputEncoder.GetBytes(decodedInput);
            return output;
        }
    }
}
