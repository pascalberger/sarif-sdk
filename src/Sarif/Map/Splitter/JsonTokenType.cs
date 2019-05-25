namespace Microsoft.CodeAnalysis.Sarif.Map.Splitter
{
    /// <summary>
    ///  JsonTokenType is the type of the next token found.
    /// </summary>
    /// <remarks>
    ///  Illegal must be zero to allow uninitialized values in the map to be illegal.
    ///  All valid separators must be at the end, to allow quickly identifying separators.
    ///  Whitespace and EndOfFile must be just before separators to allow quickly identifying (WhitespaceOrSeparator).
    /// </remarks>
    public enum JsonTokenType : byte
    {
        Illegal = 0,
        Comment,
        Null,
        False,
        True,
        Number,
        String,
        Whitespace,
        EndOfFile,
        StartObject,
        EndObject,
        StartArray,
        EndArray,
        NameSeparator,
        ValueSeparator
    }
}
