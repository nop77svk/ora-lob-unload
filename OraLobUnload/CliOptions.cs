#pragma warning disable S3237
namespace NoP77svk.OraLobUnload;

using System;
using System.Text;

using CommandLine;

using NoP77svk.OraLobUnload.DataReaders;
using NoP77svk.OraLobUnload.OracleStuff;

internal class CliOptions
{
    private readonly OracleConnectStringParsed _dbLogonParsed = new OracleConnectStringParsed();

    [Option('u', "logon", Required = true, HelpText = "\nFull database connection string in form\n<username>[/<password>]@<database>[ as sysdba| as sysoper]")]
    public string? DbLogonFull
    {
        get => _dbLogonParsed.FullConnectString;
        set { _dbLogonParsed.FullConnectString = value ?? string.Empty; }
    }

    [Option('i', "input", Required = false, HelpText = "Input script file\nIf not provided, stdin is used")]
    public string? InputFile { get; set; }

    [Option('t', "input-content", Group = "Input Content", Required = false, HelpText = "\nInput script file content type\n(One of) query, out-ref-cursor, tables, implicit-cursors")]
    public string? InputFileContentDesc
    {
        get => InputFileContentType switch
        {
            InputContentType.Select => "query",
            InputContentType.OutRefCursor => "out-ref-cursor",
            InputContentType.Tables => "tables",
            InputContentType.ImplicitCursors => "implicit-cursors",
            _ => throw new ArgumentOutOfRangeException($"Don''t know how to interpret file content type {InputFileContentType}")
        };

        set
        {
            InputFileContentType = value?.Trim()?.ToLower()?.Replace("-", string.Empty)?.Replace("_", string.Empty) switch
            {
                "select" or "query" => InputContentType.Select,
                "outrefcursor" or "outcursor" or "refcursor" or "cursor" => InputContentType.OutRefCursor,
                "table" or "tables" => InputContentType.Tables,
                "implicitcursors" or "implicitcursor" or "implicit" => InputContentType.ImplicitCursors,
                _ => throw new ArgumentOutOfRangeException($"Don''t know how to interpret file content type {value}")
            };
        }
    }

    [Option('q', "input-content-query", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=query")]
    public bool IsInputFileContentSelect
    {
        get => InputFileContentType == InputContentType.Select;
        set => InputFileContentType = InputContentType.Select;
    }

    [Option('c', "input-content-cursor", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=out-ref-cursor")]
    public bool IsInputFileContentOutCursor
    {
        get => InputFileContentType == InputContentType.OutRefCursor;
        set => InputFileContentType = InputContentType.OutRefCursor;
    }

    [Option("input-content-tables", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=tables")]
    public bool IsInputScriptTypeTables
    {
        get => InputFileContentType == InputContentType.Tables;
        set => InputFileContentType = InputContentType.Tables;
    }

    [Option('m', "input-content-implicit-cursors", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=implicit-cursors")]
    public bool IsInputScriptTypeImplicit
    {
        get => InputFileContentType == InputContentType.ImplicitCursors;
        set => InputFileContentType = InputContentType.ImplicitCursors;
    }

    [Option('o', "output", Required = false, HelpText = "Output folder")]
    public string? OutputFolder { get; set; }

    [Option('x', "output-file-extension", Required = false, HelpText = "Output file extension to be appended to the file name if not already appended by DB")]
    public string? OutputFileExtension { get; set; }

    [Option("clob-output-charset", Required = false, Default = "utf-8", HelpText = "\nConvert CLOBs automatically to target charset")]
    public string? OutputEncodingId { get; set; }

    [Option("file-name-column-ix", Required = false, Default = 1, HelpText = "\nFile name is in column #x of the data set(s) being read")]
    public int FileNameColumnIndex { get; set; }

    [Option("lob-column-ix", Required = false, Default = 2, HelpText = "\nLOB contents are in column #x of the data set(s) being read")]
    public int LobColumnIndex { get; set; }

    [Option("lob-init-fetch-size", Required = false, Default = "0", HelpText = "\n(Internal) Initial LOB fetch size\nUse \"K\" or \"M\" suffixes to denote 1KB or 1MB sizes respectively")]
    internal string? LobInitFetchSize { get; set; }

    internal OracleConnectStringParsed DbLogon
    {
        get => _dbLogonParsed;
    }

    internal Encoding OutputEncoding => string.IsNullOrEmpty(OutputEncodingId)
        ? new UTF8Encoding(false, false)
        : Encoding.GetEncoding(OutputEncodingId);

    internal InputContentType InputFileContentType { get; set; }

    internal int GetLobInitFetchSizeB()
    {
        if (string.IsNullOrEmpty(LobInitFetchSize))
        {
            return 65536;
        }

        string lobFetchWoUnit = LobInitFetchSize[0..^1];
        if (LobInitFetchSize.EndsWith("K", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToInt32(lobFetchWoUnit) * 1024;
        }
        else if (LobInitFetchSize.EndsWith("M", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024;
        }
        else if (LobInitFetchSize.EndsWith("G", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024 * 1024;
        }

        throw new CliOptionException($"Unrecognized unit of LOB fetch size \"{LobInitFetchSize}\"");
    }
}
