namespace OraLobUnload
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    internal class CharsetEncoderForClob : ICryptoTransform
    {
        private readonly Encoding utf16decoder;

        public CharsetEncoderForClob(Encoding encoder, bool sourceBigEndian = false, bool sourceHasBOM = false, int inputBufferSizeInChars = 262144)
        {
            if (inputBufferSizeInChars <= 0)
                throw new ArgumentOutOfRangeException($"Illegal input buffer size of \"{inputBufferSizeInChars}\" characters");
            InputBlockSize = inputBufferSizeInChars * 2;

            this.utf16decoder = new UnicodeEncoding(sourceBigEndian, sourceHasBOM);
            this.Encoder = encoder;
        }

        public Encoding Encoder { get; }

        public bool CanReuseTransform => false;

        public bool CanTransformMultipleBlocks => false;

        public int InputBlockSize { get; }

        public int OutputBlockSize => InputBlockSize / this.utf16decoder.GetMaxByteCount(1) * this.Encoder.GetMaxByteCount(1);

        public void Dispose()
        {
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            var inputString = this.utf16decoder.GetString(inputBuffer, inputOffset, inputCount);
            var outputBytes = this.Encoder.GetBytes(inputString);
            outputBytes.CopyTo(outputBuffer, outputOffset);
            return outputBytes.Length;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var inputString = this.utf16decoder.GetString(inputBuffer, inputOffset, inputCount);
            var outputBytes = this.Encoder.GetBytes(inputString);
            return outputBytes;
        }
    }
}
