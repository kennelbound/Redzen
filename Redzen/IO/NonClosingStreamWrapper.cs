﻿/* ****************************************************************************
 * This file is part of the Redzen code library.
 *
 * Copyright 2015 Colin D. Green (colin.green1@gmail.com)
 *
 * This software is issued under the MIT License.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
using System;
using System.IO;
using System.Runtime.Remoting;

namespace Redzen.IO
{
    /// <summary>
    /// Wraps a stream and prevents calls to Close() and Dispose() from being made on it.
    /// This is useful for other classes that wrap a stream but have no option to leave
    /// the wrapped stream open upon Dispose(), e.g. CryptoStream and BinaryWriter.
    /// 
    /// Note. Later versions of the .NET framework have added a 'leaveOpen' option
    /// to some classes. Check before using this class.
    /// </summary>
    public class NonClosingStreamWrapper : Stream
    {
        #region Instance Fields
        Stream _innerStream;
        bool isClosed=false;
        #endregion

        #region Constructor
        /// <summary>
        /// Construct with the provided stream to be wrapped.
        /// </summary>
        /// <param name="stream">The stream to be wrapped.</param>
        public NonClosingStreamWrapper(Stream stream)
        {
            _innerStream = stream;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the wrapped stream.
        /// </summary>
        public Stream InnerStream
        {
            get { return _innerStream; }
        }

        #endregion

        #region Properties [Overrides]

        /// <summary>
        /// Indicates whether or not the underlying stream can be read from.
        /// </summary>
        public override bool CanRead
        {
            get { return isClosed ? false : _innerStream.CanRead; }
        }

        /// <summary>
        /// Indicates whether or not the underlying stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return isClosed ? false : _innerStream.CanSeek; }
        }

        /// <summary>
        /// Indicates whether or not the underlying stream can be written to.
        /// </summary>
        public override bool CanWrite
        {
            get { return isClosed ? false : _innerStream.CanWrite; }
        }

        /// <summary>
        /// Returns the length of the underlying stream.
        /// </summary>
        public override long Length
        {
            get
            {
                CheckClosed();
                return _innerStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the underlying stream.
        /// </summary>
        public override long Position
        {
            get
            {
                CheckClosed();
                return _innerStream.Position;
            }
            set
            {
                CheckClosed();
                _innerStream.Position = value;
            }
        }

        #endregion

        #region Public Methods [Overrides]

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into. </param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read. </param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns> An IAsyncResult that represents the asynchronous read, which could still be pending.
        /// </returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckClosed();
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in buffer from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An IAsyncResult that represents the asynchronous write, which could still be pending.</returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            CheckClosed();
            return _innerStream.BeginWrite (buffer, offset, count, callback, state);
        }

        /// <summary>
        /// This method is not proxied to the underlying stream; instead, the wrapper is marked as unusable for
        /// other (non-close/Dispose) operations. The underlying stream is flushed if the wrapper wasn't closed
        /// before this call.
        /// </summary>
        public override void Close()
        {
            if(!isClosed) {
                _innerStream.Flush();
            }
            isClosed = true;			
        }

        /// <summary>
        /// Not implemented. Throws NotSupportedException.
        /// </summary>
        /// <param name="requestedType">The Type of the object that the new ObjRef will reference.</param>
        /// <returns>Throws NotSupportedException.</returns>
        public override ObjRef CreateObjRef(Type requestedType)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>The number of bytes read from the stream, between zero (0) and the number of bytes you
        /// requested. Streams only return zero (0) at the end of the stream, otherwise, they should block
        /// until at least one byte is available.</returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            CheckClosed();
            return _innerStream.EndRead(asyncResult);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            CheckClosed();
            _innerStream.EndWrite (asyncResult);
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            CheckClosed();
            _innerStream.Flush();
        }

        /// <summary>
        /// Not implemented. Throws NotSupportedException.
        /// </summary>
        /// <returns>Throws NotSupportedException.</returns>
        public override object InitializeLifetimeService()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads a sequence of bytes from the underlying stream and advances the 
        /// position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains 
        /// the specified byte array with the values between offset and (offset + count- 1) replaced
        /// by the bytes read from the underlying source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data 
        /// read from the underlying stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the underlying stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of 
        /// bytes requested if that many bytes are not currently available, or zero (0) if the end of the 
        /// stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckClosed();
            return _innerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns
        /// -1 if at the end of the stream.</summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        public override int ReadByte()
        {
            CheckClosed();
            return _innerStream.ReadByte();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type SeekOrigin indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the underlying stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckClosed();
            return _innerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the underlying stream.
        /// </summary>
        /// <param name="value">The desired length of the underlying stream in bytes.</param>
        public override void SetLength(long value)
        {
            CheckClosed();
            _innerStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the underlying stream and advances the current position within the stream 
        /// by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the underlying stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the underlying stream.</param>
        /// <param name="count">The number of bytes to be written to the underlying stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckClosed();
            _innerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value)
        {
            CheckClosed();
            _innerStream.WriteByte(value);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Throws an InvalidOperationException if the wrapper is closed.
        /// </summary>
        private void CheckClosed()
        {
            if(isClosed) {
                throw new InvalidOperationException("The stream has been closed.");
            }
        }

        #endregion
    }
}
