namespace TheHive.Api.Test.Support;

/// <summary>A read-once, forward-only stream (like a network response stream): it cannot seek and reports how often it was read to the end.</summary>
internal sealed class NonSeekableStream(byte[] data) : Stream
{
	private int _position;

	public int TimesDrained { get; private set; }

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotSupportedException();

	public override long Position
	{
		get => throw new NotSupportedException();
		set => throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		var read = Math.Min(count, data.Length - _position);
		Array.Copy(data, _position, buffer, offset, read);
		_position += read;
		if (read == 0)
		{
			TimesDrained++;
		}

		return read;
	}

	public override void Flush()
	{
		// Nothing to flush: the stream is read-only, and Stream.Flush must not throw on a stream that cannot be written.
	}

	public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
