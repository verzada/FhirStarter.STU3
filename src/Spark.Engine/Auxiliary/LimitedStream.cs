using System;
using System.IO;

namespace Spark.Engine.Auxiliary
{
    public class LimitedStream : Stream
    {
        /// <summary>
        /// Creates a write limit on the underlying <paramref name="stream"/> of <paramref name="sizeLimitInBytes"/>, which has a default of 2048 (2kB).
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sizeLimitInBytes"></param>
        public LimitedStream(Stream stream, long sizeLimitInBytes = 2048)
        {
            _innerStream = stream;
            _sizeLimitInBytes = sizeLimitInBytes;
        }

        private readonly Stream _innerStream;

        private readonly long _sizeLimitInBytes = 2048;
        public long SizeLimitInBytes => _sizeLimitInBytes;

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite && _innerStream.Length < _sizeLimitInBytes;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get
            {
                return _innerStream.Position;
            }

            set
            {
                _innerStream.Position = value;
            }
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int bytesToBeAdded = Math.Min(buffer.Length - offset, count);
            if (Length + bytesToBeAdded <= _sizeLimitInBytes)
            {
                _innerStream.Write(buffer, offset, count);
            }
            else
            {
                throw new ArgumentOutOfRangeException("buffer", String.Format("Adding {0} bytes to the stream would exceed the size limit of {1} bytes.", bytesToBeAdded, _sizeLimitInBytes));
            }
        }
    }
}
