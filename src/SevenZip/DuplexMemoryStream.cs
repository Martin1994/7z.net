using SevenZip;

public class SingularMemoryStreamExtractCallback : IArchiveExtractCallback
{
    private readonly TaskCompletionSource<Stream> _streamPromise = new();
    public Task<Stream> ResultStream => _streamPromise.Task;

    public long PhysicalSize;

    public SingularMemoryStreamExtractCallback()
    {
    }

    public void Fail(Exception ex)
    {
        _streamPromise.SetException(ex);
    }

    public void SetTotal(ulong size)
    {
    }

    public void SetCompleted(in ulong size)
    {
    }

    public Stream? GetStream(uint index, NAskMode askExtractMode)
    {
        if (askExtractMode != NAskMode.kExtract) {
            return null;
        }

        var buffer = new Memory<byte>(new byte[PhysicalSize]);
        var outStream = new OutStream(buffer);
        var inStream = new InStream(outStream, buffer);

        _streamPromise.SetResult(inStream);

        return outStream;
    }

    public void PrepareOperation(NAskMode askExtractMode)
    {
    }

    public void SetOperationResult(NOperationResult opRes)
    {
        // TODO: propagate result to in-stream
    }

    private class OutStream : Stream
    {
        private readonly Memory<byte> _buffer;

        private long _position = 0;
        private long _waitingPosition = 0;
        private TaskCompletionSource? _waitingPromise = null;

        public OutStream(Memory<byte> buffer)
        {
            _buffer = buffer;
        }

        private readonly object _positionLock = new();

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _buffer.Length;

        public override long Position { get => _position; set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            // TODO: >2G?
            buffer.CopyTo(_buffer.Slice((int)_position, buffer.Length).Span);

            lock (_positionLock)
            {
                _position += buffer.Length;

                if (_waitingPromise != null && _position >= _waitingPosition)
                {
                    var promise = _waitingPromise;
                    _waitingPromise = null;
                    promise.SetResult();
                }
            }
        }

        public void WaitFor(long position)
        {
            if (_waitingPromise != null)
            {
                throw new NotSupportedException("This method is not thread safe.");
            }

            Task promise;
            lock (_positionLock)
            {
                if (position <= _position)
                {
                    return;
                }

                _waitingPosition = position;
                _waitingPromise = new();
                promise = _waitingPromise.Task;
            }
            promise.Wait();
        }
    }

    private class InStream : Stream
    {
        private readonly OutStream _is;
        private readonly Memory<byte> _buffer;
        private long _position; // TODO: >2G?

        public InStream(OutStream inStream, Memory<byte> buffer)
        {
            _is = inStream;
            _buffer = buffer;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _buffer.Length;

        public override long Position { get => _position; set => Seek(value); }

        public override void Flush() => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(new Span<byte>(buffer, offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            // TODO: >2G?
            var count = Math.Min(buffer.Length, _buffer.Length - (int)_position);
            buffer = buffer.Slice(0, count);

            _is.WaitFor(_position + count);

            _buffer.Slice((int)_position, count).Span.CopyTo(buffer);
            _position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                default:
                case SeekOrigin.Begin:
                    return Seek(offset);

                case SeekOrigin.Current:
                    return Seek(_position + offset);

                case SeekOrigin.End:
                    return Seek(_buffer.Length + offset);
            }
        }

        private long Seek(long offset)
        {
            _position = offset;
            return _position;
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

}
