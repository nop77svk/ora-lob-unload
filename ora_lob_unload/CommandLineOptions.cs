namespace NoP77svk.OraLobUnload;

using System;
using System.Text;
using CommandLine;
using NoP77svk.OraLobUnload.ConnectStringParser;
using NoP77svk.OraLobUnload.DataReaders;

internal class CommandLineOptions
{
    private readonly OracleConnectStringParsed _dbLogonParsed = new OracleConnectStringParsed();

    [Option('u', "logon", Required = true, HelpText = "\nFull database connection string in form\n<username>[/<password>]@<database>[ as sysdba| as sysoper]")]
    public string? DbLogonFull
    {
        get => _dbLogonParsed.FullConnectString;
        set { _dbLogonParsed.FullConnectString = value ?? ""; }
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
            InputFileContentType = value?.Trim()?.ToLower()?.Replace("-", "")?.Replace("_", "") switch
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
    public bool InputFilecontentSelect
    {
        get => InputFileContentType == InputContentType.Select;
        set
        {
            InputFileContentType = InputContentType.Select;
        }
    }

    [Option('c', "input-content-cursor", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=out-ref-cursor")]
    public bool InputFileContentOutCursor
    {
        get => InputFileContentType == InputContentType.OutRefCursor;
        set
        {
            InputFileContentType = InputContentType.OutRefCursor;
        }
    }

    [Option("input-content-tables", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=tables")]
    public bool InputScriptTypeTables
    {
        get => InputFileContentType == InputContentType.Tables;
        set
        {
            InputFileContentType = InputContentType.Tables;
        }
    }

    [Option('m', "input-content-implicit-cursors", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=implicit-cursors")]
    public bool InputScriptTypeImplicit
    {
        get => InputFileContentType == InputContentType.ImplicitCursors;
        set
        {
            InputFileContentType = InputContentType.ImplicitCursors;
        }
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

    /*
    [Option("db", Required = false, HelpText = "Database (either as a TNS alias or an EzConnect string) to connect to")]
    internal string? DbService
    {
        get => _dbLogonParsed.DbService;
        set { _dbLogonParsed.DbService = value ?? ""; }
    }


    [Option("user", Required = false, HelpText = "Database user name to connect to")]
    internal string? DbUser
    {
        get => _dbLogonParsed.User;
        set { _dbLogonParsed.User = value ?? ""; }
    }

    [Option("pass", Required = false, HelpText = "The connecting database user's password")]
    internal string? DbPassword
    {
        get => _dbLogonParsed.Password;
        set { _dbLogonParsed.Password = value ?? ""; }
    }

    internal OracleUserConnectRole DbUserRole
    {
        get => _dbLogonParsed.SpecialRole;
        set { _dbLogonParsed.SpecialRole = value; }
    }
    */

    internal Encoding OutputEncoding => OutputEncodingId switch
    {
        null or "" => new UTF8Encoding(false, false),
        _ => Encoding.GetEncoding(OutputEncodingId)
    };

    internal int LobInitFetchSizeB
    {
        get
        {
            if (LobInitFetchSize is null or "")
            {
                return 65536;
            }
            else
            {
                string lobFetchWoUnit = LobInitFetchSize[0..^1];
                if (LobInitFetchSize.EndsWith("K", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToInt32(lobFetchWoUnit) * 1024;
                else if (LobInitFetchSize.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024;
                else if (LobInitFetchSize.EndsWith("G", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024 * 1024;
                else
                    throw new ArgumentOutOfRangeException(nameof(LobInitFetchSize), $"Unrecognized unit of LOB fetch size \"{LobInitFetchSize}\"");
            }
        }
    }

    internal InputContentType InputFileContentType { get; set; }
}
