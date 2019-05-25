using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Sarif.Map.Splitter
{
    /// <summary>
    ///  BufferedReader wraps a stream, in-memory byte array, or array slice.
    ///  Users work with absolute offsets only which are mapped to the available buffer.
    ///  'End' is used to check for AND read remaining content when needed.
    ///  
    ///  Usage:
    ///  long tsvCellStart = reader.Position;
    ///  
    ///  while (!reader.End)
    ///  {
    ///      if (reader.Current == "\t") { break; }
    ///      reader.Position++;
    ///  }
    ///  
    ///  long tsvCellEnd = reader.Position - 1;
    ///  byte[] value = 
    ///  
    ///  reader.ConsumeTo(tsvCellEnd); 
    /// </summary>
    internal class BufferedReader : IDisposable
    {
        private Stream _source;
        private bool _sourceAtEnd;

        private byte[] _buffer;
        private long _bytesBeforeBuffer;
        private long _bytesRead;
        private long _bytesConsumed;

        /// <summary>
        ///  Get or Set the current position being examined.
        /// </summary>
        public long Position;

        /// <summary>
        ///  Returns byte[Position] from the source.
        /// </summary>
        public byte Current => _buffer[Position - _bytesBeforeBuffer];

        /// <summary>
        ///  Return whether the data there is any more data after Position.
        ///  Will internally read more data if it is available.
        /// </summary>
        public bool End => Position >= _bytesRead && !Read();

        /// <summary>
        ///  Build a BufferedReader for a given stream.
        /// </summary>
        /// <param name="source">Source stream to read from</param>
        /// <param name="bufferSize">Buffer Size in bytes to start with</param>
        public BufferedReader(Stream source, int bufferSize = 64 * 1024)
        {
            _source = source;
            _buffer = new byte[bufferSize];
        }

        /// <summary>
        ///  Build a BufferedReader for an in-memory byte array or slice.
        /// </summary>
        /// <param name="source">byte[] containing content</param>
        /// <param name="index">Index where slice begins</param>
        /// <param name="length">Length in bytes of slice</param>
        public BufferedReader(byte[] source, int index, int length)
        {
            _sourceAtEnd = true;
            _buffer = source;
            _bytesBeforeBuffer = -index;
            _bytesRead = length;
            _bytesConsumed = 0;
            Position = 0;
        }

        /// <summary>
        ///  Notify the Reader that bytes up to position have been consumed
        ///  and are no longer needed.
        /// </summary>
        /// <param name="position">Inclusive position no longer needed</param>
        public void ConsumeTo(long position)
        {
            if (position < _bytesConsumed) { throw new ArgumentOutOfRangeException($"BufferedReader cannot consume to {position:n0}; {_bytesConsumed:n0} already consumed."); }
            if (position > _bytesRead) { throw new ArgumentOutOfRangeException($"BufferedReader cannot consume to {position:n0}; it's only read up to {_bytesRead:n0}."); }
            _bytesConsumed = position;
        }

        /// <summary>
        ///  Copy a range from this BufferedReader to another array.
        /// </summary>
        /// <param name="other">Array to copy to</param>
        /// <param name="copyToIndex">Index to copy to</param>
        /// <param name="position">Absolute byte offset (position) to copy from</param>
        /// <param name="length">Number of bytes to copy</param>
        public void CopyTo(byte[] other, int copyToIndex, long position, long length)
        {
            if (position < _bytesConsumed) { throw new ArgumentOutOfRangeException($"BufferedReader cannot copy from {position:n0}; {_bytesConsumed:n0} already consumed."); }
            if (position + length > _bytesRead) { throw new ArgumentOutOfRangeException($"BufferedReader cannot copy to {(position + length):n0}; it's only read up to {_bytesRead:n0} already consumed."); }

            Buffer.BlockCopy(_buffer, (int)(position - _bytesBeforeBuffer), other, copyToIndex, (int)length);
        }

        /// <summary>
        ///  Get the .NET string representation of a UTF8 range.
        /// </summary>
        /// <param name="position">Absolute byte offset (position) to copy from</param>
        /// <param name="length">Number of bytes to copy</param>
        /// <returns>String representation of range</returns>
        public string ToString(long position, long length)
        {
            if (position < _bytesConsumed) { throw new ArgumentOutOfRangeException($"BufferedReader cannot copy from {position:n0}; {_bytesConsumed:n0} already consumed."); }
            if (position + length > _bytesRead) { throw new ArgumentOutOfRangeException($"BufferedReader cannot copy to {(position + length):n0}; it's only read up to {_bytesRead:n0} already consumed."); }

            return Encoding.UTF8.GetString(_buffer, (int)(position - _bytesBeforeBuffer), (int)length);
        }

        /// <summary>
        ///  Refill the buffer with more data from the source stream, if any.
        /// </summary>
        /// <returns>False if everything was previously returned</returns>
        private bool Read()
        {
            if (_sourceAtEnd) { return false; }

            byte[] toFill = _buffer;

            // Count bytes not yet consumed in the buffer
            long bytesToSave = _bytesRead - _bytesConsumed;
            long bytesToSaveIndex = (_bytesRead - _bytesBeforeBuffer) - bytesToSave;

            // If the buffer is too small, grow it
            if (_buffer.Length < 2 * bytesToSave)
            {
                toFill = new byte[2 * _buffer.Length];
            }

            // Save the not consumed bytes at the beginning of the buffer
            if (bytesToSave > 0)
            {
                Buffer.BlockCopy(_buffer, (int)bytesToSaveIndex, toFill, 0, (int)bytesToSave);
            }

            // Fill the remaining buffer
            int bufferBytesAvailable = (int)(toFill.Length - bytesToSave);
            int newBytesRead = _source.Read(toFill, (int)bytesToSave, bufferBytesAvailable);

            // Update whether we're out of bytes
            _sourceAtEnd = (newBytesRead < bufferBytesAvailable);

            // Update how much we've read in total
            _bytesRead += newBytesRead;

            // Update where the buffer begins
            _bytesBeforeBuffer = _bytesConsumed;

            // Verify we kept correct count

            return true;
        }

        public void Dispose()
        {
            _source?.Dispose();
            _source = null;
        }
    }
}
