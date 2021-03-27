namespace SK.NoP77svk.OraLobUnload
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using CommandLine;
    using SK.NoP77svk.OraLobUnload.InputSqlCommands;

    internal class CommandLineOptions
    {
        [Option('u', "logon", Required = true, HelpText = "\nFull database connection string in form <username>/<password>@<database>\nNote: \"as sysdba\" is not supported (yet)")]
        public string? DbLogonFull
        {
            get => $"{DbUser}/{DbPassword}@{DbService}";
            set
            {
                if (value is null)
                    return;

                Match m = Regex.Match(value, @"^\s*([^/@ ]*)(\s*/\s*([^@ ]*))?\s*@\s*(\S*)\s*$");
                if (m.Success)
                {
                    DbUser = m.Groups[1].Value;
                    DbPassword = m.Groups[3].Value;
                    DbService = m.Groups[4].Value;
                }
                else
                {
                    throw new ArgumentException($"\"{value}\" is not a valid Oracle connection string");
                }
            }
        }

        [Option('i', "input", Required = false, HelpText = "Input script file\nIf not provided, stdin is used")]
        public string? InputFile { get; set; }

        [Option('t', "input-content", Group = "Input Content", Required = false, HelpText = "\nInput script file content type\n(One of) query, out-ref-cursor, tables, implicit-cursors")]
        public string? InputFileContentDesc
        {
            get => InputFileContent switch
            {
                InputContentType.Select => "query",
                InputContentType.OutRefCursor => "out-ref-cursor",
                InputContentType.Tables => "tables",
                InputContentType.ImplicitCursors => "implicit-cursors",
                _ => throw new ArgumentOutOfRangeException($"Don''t know how to interpret file content type {InputFileContent}")
            };

            set
            {
                InputFileContent = value?.Trim()?.ToLower()?.Replace("-", "")?.Replace("_", "") switch
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
            get => InputFileContent == InputContentType.Select;
            set
            {
                InputFileContent = InputContentType.Select;
            }
        }

        [Option('c', "input-content-cursor", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=out-ref-cursor")]
        public bool InputFileContentOutCursor
        {
            get => InputFileContent == InputContentType.OutRefCursor;
            set
            {
                InputFileContent = InputContentType.OutRefCursor;
            }
        }

        [Option("input-content-tables", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=tables")]
        public bool InputScriptTypeTables
        {
            get => InputFileContent == InputContentType.Tables;
            set
            {
                InputFileContent = InputContentType.Tables;
            }
        }

        [Option('m', "input-content-implicit-cursors", Group = "Input Content", Required = false, HelpText = "\nShorthand for --input-content=implicit-cursors")]
        public bool InputScriptTypeImplicit
        {
            get => InputFileContent == InputContentType.ImplicitCursors;
            set
            {
                InputFileContent = InputContentType.ImplicitCursors;
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

        [Option("lob-init-fetch-size", Required = false, Default = "64K", HelpText = "\n(Internal) Initial LOB fetch size\nUse \"K\" or \"M\" suffixes to denote 1KB or 1MB sizes respectively")]
        internal string? LobInitFetchSize { get; set; }

        [Option("db", Required = false, HelpText = "Database (either as a TNS alias or an EzConnect string) to connect to")]
        internal string? DbService { get; set; }

        [Option("user", Required = false, HelpText = "Database user name to connect to")]
        internal string? DbUser { get; set; }

        [Option("pass", Required = false, HelpText = "The connecting database user's password")]
        internal string? DbPassword { get; set; }

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

        internal InputContentType InputFileContent { get; set; }
    }
}
