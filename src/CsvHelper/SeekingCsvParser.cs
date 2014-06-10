using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;

namespace CsvHelper
{
    /// <summary>
    /// CsvParser with seeking support <br />
    /// Useful if you want to index into a large CSV file
    /// </summary>
    public class SeekingCsvParser : CsvParser
    {
        private long _initOffset = 0;
        private long _bytePositionRecord;
        private StreamReader _streamReader;

        public SeekingCsvParser(TextReader reader, int initbytes = 0) : this(reader, new CsvConfiguration(), initbytes)
        {            
        }

        public SeekingCsvParser(TextReader reader, CsvConfiguration configuration, int initbytes) : base(reader, configuration)
        {
            if (!configuration.CountBytes)
                throw new ArgumentException("Expected configuration.CountBytes to be set to true");

            var sr = reader as StreamReader;
            if (sr == null)
                throw new ArgumentException("Provided reader is not a StreamReader");
            if (!sr.BaseStream.CanSeek)
                throw new ArgumentException("Underlying stream doesn't support seeking");
            if (sr.BaseStream.Position < 0)
                throw new ArgumentException("Underlying stream reports offset < 0");

            _initOffset = initbytes;
            _bytePositionRecord = 0;
            _streamReader = sr;
        }

        /// <summary>
        /// Get the byte position of the start of the line for the current record
        /// </summary>
        public virtual long BytePositionRecord
        {
            get { return _bytePositionRecord; }
            protected set { _bytePositionRecord = value; }
        }

        /// <summary>
        /// Get the byte position of the start of the line for the current record, offset for initial stream position
        /// </summary>
        public virtual long BytePositionRecordRaw
        {
            get { return _bytePositionRecord + _initOffset; }
        }

        /// <summary>
        /// Get the byte position that the parser is currently on, offset for initial stream position
        /// </summary>
        public virtual long BytePositionRaw
        {
            get { return base.BytePosition + _initOffset; }
        }


        /// <summary>
        /// Reads the next line.
        /// </summary>
        /// <returns>The line separated into fields.</returns>
        protected override string[] ReadLine()
        {
            _bytePositionRecord = BytePosition;
            return base.ReadLine();
        }

        /// <summary>
        /// Advance to the given absolute position in the underlying stream. <br />
        /// Warning; the given position is assumed to coincide with the start of a line. If it does not, you may get unexpected results. <br />
        /// After seeking, the record number will be reset to 0
        /// </summary>
        /// <param name="position">A byte offset position to seek to in the underlying stream.</param>
        public void Seek(long position)
        {
            Seek(position, SeekOrigin.Begin);
        }

        /// <summary>
        /// Advance to the given absolute position in the underlying stream. <br />
        /// Warning; the given position is assumed to coincide with the start of a line. If it does not, you may get unexpected results.
        /// After seeking, the record number will be reset to 0
        /// </summary>
        /// <param name="position">A byte offset position to seek to in the underlying stream.</param>
        /// <param name="origin">Reference point</param>
        private void Seek(long position, SeekOrigin origin)
        {
            base.CheckDisposed();
            _streamReader.BaseStream.Seek(position, origin);
            _streamReader.DiscardBufferedData();
            _initOffset = position; 
            base.Reset();
        }
    }
}
