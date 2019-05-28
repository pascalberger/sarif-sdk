using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.CodeAnalysis.Sarif.Map.Splitter
{
    public struct JsonToken
    {
        public long WhitespaceStartIndex;
        public long ValueStartIndex;
        public long ValueEndIndex;
        public JsonTokenType TokenType;

        public long WhitespaceLength => ValueStartIndex - WhitespaceStartIndex;
        public long ValueLength => 1 + ValueEndIndex - ValueStartIndex;

        public JsonToken(long whitespaceStart, long valueStart, long valueEnd, JsonTokenType tokenType)
        {
            this.WhitespaceStartIndex = whitespaceStart;
            this.ValueStartIndex = valueStart;
            this.ValueEndIndex = valueEnd;
            this.TokenType = tokenType;
        }
    }

    public class JsonSplitter : IDisposable
    {
        private const byte FirstWhitespaceOrSeparator = (byte)JsonTokenType.Whitespace;
        private const int DecodeBatchSize = 128;
        private static JsonTokenType[] _map;

        private BufferedReader _reader;

        public JsonToken Current { get; private set; }

        private JsonToken[] _decoded;
        private int _decodedNextIndex;
        private int _decodedCount;

        public JsonSplitter(Stream stream) : this(new BufferedReader(stream))
        { }

        internal JsonSplitter(BufferedReader reader)
        {
            InitializeMap();
            _reader = reader;
            _decoded = new JsonToken[DecodeBatchSize];
        }

        /// <summary>
        ///  Build a map of all UTF8 bytes which may appear in JSON outside strings.
        /// </summary>
        private static void InitializeMap()
        {
            if (_map != null) { return; }

            JsonTokenType[] map = new JsonTokenType[256];

            map[(byte)' '] = JsonTokenType.Whitespace;
            map[(byte)'\t'] = JsonTokenType.Whitespace;
            map[(byte)'\r'] = JsonTokenType.Whitespace;
            map[(byte)'\n'] = JsonTokenType.Whitespace;

            map[(byte)'n'] = JsonTokenType.Null;
            map[(byte)'f'] = JsonTokenType.False;
            map[(byte)'t'] = JsonTokenType.True;

            map[(byte)'-'] = JsonTokenType.Number;
            for (char i = '0'; i <= '9'; ++i)
            {
                map[(byte)i] = JsonTokenType.Number;
            }

            map[(byte)'"'] = JsonTokenType.String;
            map[(byte)'{'] = JsonTokenType.StartObject;
            map[(byte)'}'] = JsonTokenType.EndObject;
            map[(byte)'['] = JsonTokenType.StartArray;
            map[(byte)']'] = JsonTokenType.EndArray;
            map[(byte)':'] = JsonTokenType.NameSeparator;
            map[(byte)','] = JsonTokenType.ValueSeparator;

            _map = map;
        }

        public bool Next()
        {
            if (_decodedNextIndex == _decodedCount)
            {
                Split();
                if (_decodedCount == 0) { return false; }
            }

            Current = _decoded[_decodedNextIndex++];
            return true;
        }

        public string CurrentValueString()
        {
            return _reader.ToString(Current.ValueStartIndex, Current.ValueLength);
        }

        private void Split()
        {
            _decodedCount = 0;
            _decodedNextIndex = 0;

            if (_reader.End) { return; }

            // Mark all previous tokens consumed
            if (_reader.Position > 0) { _reader.ConsumeTo(_reader.Position - 1); }

            while (!_reader.End && _decodedCount < DecodeBatchSize)
            {
                JsonToken token;
                token.TokenType = JsonTokenType.EndOfFile;

                // Mark token start
                token.WhitespaceStartIndex = _reader.Position;

                // Read any whitespace
                while (!_reader.End)
                {
                    token.TokenType = _map[_reader.Current];
                    _reader.Position++;
                    if (token.TokenType != JsonTokenType.Whitespace) { break; }
                }

                // Stop if we ran out of file after whitespace
                if (token.TokenType == JsonTokenType.EndOfFile && token.TokenType == JsonTokenType.Whitespace) { break; }

                // Mark value start
                token.ValueStartIndex = _reader.Position - 1;

                if (token.TokenType == JsonTokenType.String)
                {
                    // If string, read until end quote
                    ReadString();
                }
                else
                {
                    // Read until whitespace or separator (end of anything else)
                    while (!_reader.End)
                    {
                        JsonTokenType type = _map[_reader.Current];
                        if ((byte)type >= FirstWhitespaceOrSeparator) { break; }
                        _reader.Position++;
                    }
                }

                // Mark value end
                token.ValueEndIndex = _reader.Position - 1;
                
                _decoded[_decodedCount++] = token;
            }
        }

        private void ReadString()
        {
            // Consume opening quote
            _reader.Position++;

            long start = _reader.Position;

            // Find the end of the string
            while (!_reader.End)
            {
                // Find the next quote
                if (_reader.Current == (byte)'"')
                {
                    long end = _reader.Position;

                    // Count backslashes before end quote
                    _reader.Position--;
                    while (_reader.Position > start && _reader.Current == (byte)'\\')
                    {
                        _reader.Position--;
                    }

                    int backslashCount = (int)((end - _reader.Position) - 1);

                    // Reset current position
                    _reader.Position = end;

                    // If count was even, quote was end of string
                    if ((backslashCount & 1) == 0) { break; }
                }

                _reader.Position++;
            }

            // Consume closing quote
            _reader.Position++;
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _reader = null;
        }
    }
}
