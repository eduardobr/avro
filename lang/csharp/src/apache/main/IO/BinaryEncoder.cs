/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.IO;

namespace Avro.IO
{
    /// <summary>
    /// Write leaf values.
    /// </summary>
    public class BinaryEncoder : Encoder
    {
        private readonly Stream Stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryEncoder"/> class without a backing
        /// stream.
        /// </summary>
        public BinaryEncoder() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryEncoder"/> class that writes to
        /// the provided stream.
        /// </summary>
        /// <param name="stream">Stream to write to.</param>
        public BinaryEncoder(Stream stream)
        {
            this.Stream = stream;
        }

        /// <summary>
        /// null is written as zero bytes
        /// </summary>
        public void WriteNull()
        {
        }

        /// <summary>
        /// true is written as 1 and false 0.
        /// </summary>
        /// <param name="b">Boolean value to write</param>
        public void WriteBoolean(bool b)
        {
            WriteByte((byte)(b ? 1 : 0));
        }

        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteInt(int value)
        {
            WriteLong(value);
        }
        /// <summary>
        /// int and long values are written using variable-length, zig-zag coding.
        /// </summary>
        /// <param name="value">Value to write</param>
        public void WriteLong(long value)
        {
            ulong n = (ulong)((value << 1) ^ (value >> 63));
            while ((n & ~0x7FUL) != 0)
            {
                WriteByte((byte)((n & 0x7f) | 0x80));
                n >>= 7;
            }
            WriteByte((byte)n);
        }

        /// <summary>
        /// A float is written as 4 bytes.
        /// The float is converted into a 32-bit integer using a method equivalent to
        /// Java's floatToIntBits and then encoded in little-endian format.
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer);
            WriteBytesInternal(buffer);
        }
        /// <summary>
        ///A double is written as 8 bytes.
        ///The double is converted into a 64-bit integer using a method equivalent to
        ///Java's doubleToLongBits and then encoded in little-endian format.
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);

            WriteByte((byte)(bits & 0xFF));
            WriteByte((byte)((bits >> 8) & 0xFF));
            WriteByte((byte)((bits >> 16) & 0xFF));
            WriteByte((byte)((bits >> 24) & 0xFF));
            WriteByte((byte)((bits >> 32) & 0xFF));
            WriteByte((byte)((bits >> 40) & 0xFF));
            WriteByte((byte)((bits >> 48) & 0xFF));
            WriteByte((byte)((bits >> 56) & 0xFF));

        }

        /// <summary>
        /// Bytes are encoded as a long followed by that many bytes of data.
        /// </summary>
        /// <param name="value"></param>
        public void WriteBytes(byte[] value)
        {
            WriteLong(value.Length);
            WriteBytesInternal(value);
        }

        /// <summary>
        /// A string is encoded as a long followed by
        /// that many bytes of UTF-8 encoded character data.
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(string value)
        {
            WriteBytes(System.Text.Encoding.UTF8.GetBytes(value));
        }

        /// <inheritdoc/>
        public void WriteEnum(int value)
        {
            WriteLong(value);
        }

        /// <inheritdoc/>
        public void StartItem()
        {
        }

        /// <inheritdoc/>
        public void SetItemCount(long value)
        {
            if (value > 0) WriteLong(value);
        }

        /// <inheritdoc/>
        public void WriteArrayStart()
        {
        }

        /// <inheritdoc/>
        public void WriteArrayEnd()
        {
            WriteLong(0);
        }

        /// <inheritdoc/>
        public void WriteMapStart()
        {
        }

        /// <inheritdoc/>
        public void WriteMapEnd()
        {
            WriteLong(0);
        }

        /// <inheritdoc/>
        public void WriteUnionIndex(int value)
        {
            WriteLong(value);
        }

        /// <inheritdoc/>
        public void WriteFixed(byte[] data)
        {
            WriteFixed(data, 0, data.Length);
        }

        /// <inheritdoc/>
        public void WriteFixed(byte[] data, int start, int len)
        {
            Stream.Write(data, start, len);
        }

        private void WriteBytesInternal(byte[] bytes)
        {
            Stream.Write(bytes, 0, bytes.Length);
        }

        private void WriteByte(byte b)
        {
            Stream.WriteByte(b);
        }

        /// <summary>
        /// Flushes the underlying stream.
        /// </summary>
        public void Flush()
        {
            Stream.Flush();
        }
    }
}
