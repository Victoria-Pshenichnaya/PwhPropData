using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel;
using System.ServiceModel.Channels;
using PwhPropData.Core.Common;

namespace PwhPropData.Core.GZipExtension
{
    public class GZipMessageEncoderFactory : MessageEncoderFactory
    {
        readonly MessageEncoder _encoder;

        //gzip magic constant value
        public const int GZIP_MAGIC = 0x1F8B;

        //The GZip encoder wraps an inner encoder
        //We require a factory to be passed in that will create this inner encoder
        public GZipMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory, bool enableCompression)
        {
            Guard.NotNull(messageEncoderFactory, "messageEncoderFactory", "A valid message encoder factory must be passed to the GZipEncoder");
            _encoder = new GZipMessageEncoder(messageEncoderFactory.Encoder, enableCompression);
        }

        //The service framework uses this property to obtain an encoder from this encoder factory
        public override MessageEncoder Encoder
        {
            get { return _encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return _encoder.MessageVersion; }
        }

        public static TextMessageEncodingBindingElement GetTextMessageEncodingBindingElement()
        {
            var encBindElm = new TextMessageEncodingBindingElement();
            encBindElm.ReaderQuotas.MaxArrayLength = int.MaxValue;
            encBindElm.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
            encBindElm.ReaderQuotas.MaxDepth = 32;
            encBindElm.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;
            encBindElm.ReaderQuotas.MaxStringContentLength = int.MaxValue;
            return encBindElm;
        }

        //This is the actual GZip encoder
        public class GZipMessageEncoder : MessageEncoder
        {
            //internal static string GZipContentType = "";
            //This implementation wraps an inner encoder that actually converts a WCF Message
            //into textual XML, binary XML or some other format. This implementation then compresses the results.
            //The opposite happens when reading messages.
            //This member stores this inner encoder.
            readonly MessageEncoder _innerEncoder;
            readonly bool _enableCompression;

            //We require an inner encoder to be supplied (see comment above)
            internal GZipMessageEncoder(MessageEncoder messageEncoder, bool enableCompression)
            {
                _innerEncoder = messageEncoder;
                _enableCompression = enableCompression;

                Guard.NotNull(messageEncoder, "messageEncoder", "A valid message encoder must be passed to the GZipEncoder");
            }

            public override string ContentType
            {
                get { return _innerEncoder.ContentType; }
            }

            public override string MediaType
            {
                get { return _innerEncoder.ContentType; }
            }

            //SOAP version to use - we delegate to the inner encoder for this
            public override MessageVersion MessageVersion
            {
                get { return _innerEncoder.MessageVersion; }
            }

            /// <summary>
            /// Helper method to compress an array of bytes 
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="bufferManager"></param>
            /// <param name="messageOffset"></param>
            /// <returns></returns>
            static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset)
            {
                //Uncommet this code if you want to compress the request...
                var memoryStream = new MemoryStream();
                memoryStream.Write(buffer.Array, 0, messageOffset);
                using (var gzStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
                {
                    gzStream.Write(buffer.Array, messageOffset, buffer.Count);
                }
                byte[] compressedBytes = memoryStream.ToArray();
                byte[] bufferedBytes = bufferManager.TakeBuffer(compressedBytes.Length);
                Array.Copy(compressedBytes, 0, bufferedBytes, 0, compressedBytes.Length);
                bufferManager.ReturnBuffer(buffer.Array);
                //ArraySegment<byte> byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, bufferedBytes.Length - messageOffset);
                //Make sure that we do not insert additional bytes - Limit the byte array to the memorystream length
                //while forming the byte array - The additional nytes will cause Tornado to send some strange exceptions !!
                var byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, (int)memoryStream.Length);
                return byteArray;
            }

            /// <summary>
            /// helper method to identify if the data is compressed in gzip format
            /// </summary>
            /// <param name="val1"></param>
            /// <param name="val2"></param>
            /// <returns></returns>
            static bool IsDataCompressed(int val1, int val2)
            {
                if (val1 != (GZIP_MAGIC >> 8) && val2 != (GZIP_MAGIC & 0xFF))
                    return false;
                return true;
            }

            /// <summary>
            /// Helper method to decompress an array of bytes
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="bufferManager"></param>
            /// <returns></returns>
            static ArraySegment<byte> DecompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
            {

                //if the buffer data is not compressed, we will return buffer without decompressing it
                if (!IsDataCompressed(Convert.ToInt32(buffer.Array.GetValue(0)), Convert.ToInt32(buffer.Array.GetValue(1))))
                    return buffer;

                var memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count - buffer.Offset);
                var decompressedStream = new MemoryStream();
                const int blockSize = 1024;
                byte[] tempBuffer = bufferManager.TakeBuffer(blockSize);
                using (var gzStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    while (true)
                    {
                        int bytesRead = gzStream.Read(tempBuffer, 0, blockSize);
                        if (bytesRead == 0)
                            break;
                        decompressedStream.Write(tempBuffer, 0, bytesRead);
                    }
                }
                bufferManager.ReturnBuffer(tempBuffer);

                byte[] decompressedBytes = decompressedStream.ToArray();
                byte[] bufferManagerBuffer = bufferManager.TakeBuffer(decompressedBytes.Length + buffer.Offset);
                Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);
                Array.Copy(decompressedBytes, 0, bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);
                var byteArray = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);
                bufferManager.ReturnBuffer(buffer.Array);

                return byteArray;
            }

            public override bool IsContentTypeSupported(string contentType)
            {
                if (contentType.Contains("\""))
                {
                    char[] splitter = { ';' };
                    contentType = contentType.Replace("\"", "");
                    string[] sSplit = contentType.Split(splitter);
                    //sSplit[1] = sSplit[1].Replace('"', ' ');
                    contentType = sSplit[0].Trim() + "; " + sSplit[1].Trim();
                    //sSplit = contentType.Split(';');
                    //contentType = sSplit[0].Trim() + sSplit[1].Trim();
                }

                if (base.IsContentTypeSupported(contentType))
                {
                    return true;
                }

                if (contentType.Length == MediaType.Length)
                {
                    return contentType.Equals(MediaType, StringComparison.OrdinalIgnoreCase);
                }

                if (contentType.StartsWith(MediaType.Split(';')[0], StringComparison.OrdinalIgnoreCase))
                    //&& (contentType[this.MediaType.Length] == ';'))
                {
                    return true;
                }
                return false;
            }

            /// <summary>
            /// One of the two main entry points into the encoder. Called by WCF to decode a buffered byte array into a Message. 
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="bufferManager"></param>
            /// <param name="contentType"></param>
            /// <returns></returns>
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                //buffer data is not compressed, we will return buffer without decompressing
                if (!IsDataCompressed(Convert.ToInt32(buffer.Array.GetValue(0)), Convert.ToInt32(buffer.Array.GetValue(1))))
                {
                    Message returnMessage = _innerEncoder.ReadMessage(buffer, bufferManager);
                    returnMessage.Properties.Encoder = this;
                    return returnMessage;
                }

                //Decompress the buffer
                ArraySegment<byte> decompressedBuffer = DecompressBuffer(buffer, bufferManager);
                //Use the inner encoder to decode the decompressed buffer
                Message returnMessageCompressed = _innerEncoder.ReadMessage(decompressedBuffer, bufferManager);
                returnMessageCompressed.Properties.Encoder = this;
                return returnMessageCompressed;
            }

            /// <summary>
            /// One of the two main entry points into the encoder. Called by WCF to encode a Message into a buffered byte array. 
            /// </summary>
            /// <param name="message"></param>
            /// <param name="maxMessageSize"></param>
            /// <param name="bufferManager"></param>
            /// <param name="messageOffset"></param>
            /// <returns></returns>
            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                //Use the inner encoder to encode a Message into a buffered byte array
                ArraySegment<byte> buffer = _innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, messageOffset);
                //Compress the resulting byte array

                bool bWriteGzipStream = false;

                if (_enableCompression)
                {
                    if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null &&
                        OperationContext.Current.IncomingMessageProperties.ContainsKey("httpRequest"))
                    {
                        foreach (object obj in OperationContext.Current.IncomingMessageProperties.Values)
                        {
                            var property = obj as HttpRequestMessageProperty;
                            if (property != null)
                            {
                                if (property.Headers.Get("Accept-Encoding") == "gzip")
                                    bWriteGzipStream = true;
                            }
                        }
                    }
                    else
                    {
                        bWriteGzipStream = true;
                    }
                }

                if (bWriteGzipStream)
                    return CompressBuffer(buffer, bufferManager, messageOffset);
                return buffer;
            }

            /// <summary>
            /// One of the two main entry points into the encoder. Called by WCF to decode a buffered byte array into a Message. 
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="maxSizeOfHeaders"></param>
            /// <param name="contentType"></param>
            /// <returns></returns>
            public override Message ReadMessage(System.IO.Stream stream, int maxSizeOfHeaders, string contentType)
            {
                //perform check to see if the data is in compressed form
                var buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                stream.Position = 0;

                if (!IsDataCompressed(buffer[0], buffer[1]))
                {
                    //data is not compressed, we will return data without attempting to decompress
                    return _innerEncoder.ReadMessage(stream, maxSizeOfHeaders);
                }
                
                //data is in gzip compressed form, we will uncompress it and send further
                var gzStream = new GZipStream(stream, CompressionMode.Decompress, true);
                return _innerEncoder.ReadMessage(gzStream, maxSizeOfHeaders);
            }

            /// <summary>
            /// One of the two main entry points into the encoder. Called by WCF to encode a Message into a buffered byte array. 
            /// </summary>
            /// <param name="message"></param>
            /// <param name="stream"></param>
            public override void WriteMessage(Message message, System.IO.Stream stream)
            {
                //currently only for PWTest we need to compress the request message, 
                // since pw server will user IIS compression, we will disable it             

                bool bWriteGzipStream = false;

                if (_enableCompression)
                {
                    //If it's a web request - HttpContext.Current will not be null - Check for Accept-Encoding header - Compress only if Accept-Encoding is Gzip
                    //HttpContext.Current.Items["Accept-Encoding"] is set to Gzip in International Message Inspector

                    if (OperationContext.Current != null && OperationContext.Current.IncomingMessageProperties != null &&
                        OperationContext.Current.IncomingMessageProperties.ContainsKey("httpRequest"))
                    {
                        foreach (object obj in OperationContext.Current.IncomingMessageProperties.Values)
                        {
                            var property = obj as HttpRequestMessageProperty;
                            if (property != null)
                            {
                                if (property.Headers.Get("Accept-Encoding") == "gzip")
                                    bWriteGzipStream = true;
                            }
                        }
                    }
                    else
                        bWriteGzipStream = true;
                }
                if (bWriteGzipStream)
                {
                    using (var gzStream = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        _innerEncoder.WriteMessage(message, gzStream);
                    }
                }
                else
                {
                    _innerEncoder.WriteMessage(message, stream);
                }
                //innerEncoder.WriteMessage(message, stream);
                // innerEncoder.WriteMessage(message, gzStream) depends on that it can flush data by flushing 
                // the stream passed in, but the implementation of GZipStream.Flush will not flush underlying
                // stream, so we need to flush here.
                stream.Flush();
            }
        }
    }
}