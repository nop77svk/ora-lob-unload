namespace SK.NoP77svk.OraLobUnload
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using CommandLine;
    using SK.NoP77svk.OraLobUnload.InputSqlCommands;

    internal class CommandLineOptions
    {
        [Option('f', "file", Required = false, HelpText = "Input SQL script file; If not provided, stdin is used")]
        public string? InputSqlScriptFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output folder")]
        public string? OutputFolder { get; set; }

        [Option('x', "output-file-extension", Required = false, HelpText = "Output file extension to be appended to the file name if not already appended by DB")]
        public string? OutputFileExtension { get; set; }

        [Option("clob-output-charset", Required = false, Default = "utf-8", HelpText = "Convert CLOBs automatically to target charset")]
        public string? OutputEncodingId { get; set; }

        [Option("file-name-column-ix", Required = false, Default = 1, HelpText = "File name is in column #x of output data set(s)")]
        public int FileNameColumnIndex { get; set; }

        [Option("lob-column-ix", Required = false, Default = 2, HelpText = "LOB contents are in column #x of output data set(s)")]
        public int LobColumnIndex { get; set; }

        [Option("lob-init-fetch-size", Required = false, Default = "64K", HelpText = "(Internal) Initial LOB fetch size; Use \"K\", \"M\" suffixes to denote 1KB and 1MB sizes respectively")]
        public string? LobFetchSize { get; set; }

        [Option('t', "use-tables", Required = false, Default = false, SetName = "in-type-table", HelpText = "Input script file contains EOLN-delimited list of tables names to query")]
        public bool InputSqlReturnTypeTable { get; set; }

        [Option('q', "use-query", Required = false, Default = false, SetName = "in-type-query", HelpText = "Input script file contains a SELECT (w/o trailing semicolon)")]
        public bool InputSqlReturnTypeSelect { get; set; }

        [Option('c', "use-cursor", Required = false, Default = false, SetName = "in-type-cursor", HelpText = "Input script file contains a PL/SQL block with a single out-bound variable of SYS_REFCURSOR type")]
        public bool InputSqlReturnTypeCursor { get; set; }

        [Option('m', "use-implicit-cursor", Required = false, Default = false, SetName = "in-type-implicit", HelpText = "Input script file contains a PL/SQL block returning arbitrary number of implicit cursors")]
        public bool InputSqlReturnTypeMultiImplicit { get; set; }

        [Option("db", Required = false, HelpText = "Database (either as a TNS alias or an EzConnect string) to connect to")]
        public string? DbService { get; set; }

        [Option("user", Required = false, HelpText = "Database user name to connect to")]
        public string? DbUser { get; set; }

        [Option("pass", Required = false, HelpText = "The connecting database user's password")]
        public string? DbPassword { get; set; }

        [Option('u', "logon", Required = false, HelpText = "Full database connection string as used by, e.g, the classic SQL*Plus")]
        public string? DbLogonFull
        {
            get => $"{DbUser}/{DbPassword}@{DbService}";
            set
            {
                if (value is null)
                    return;
                Match m = Regex.Match(value, @"^\s*([^/ ]*)\s*/\s*([^@ ]*)\s*@\s*(\S*)\s*$");
                if (m.Success)
                {
                    DbUser = m.Groups[1].Value;
                    DbPassword = m.Groups[2].Value;
                    DbService = m.Groups[3].Value;
                }
                else
                {
                    throw new ArgumentException($"\"{value}\" is not a valid Oracle connection string");
                }
            }
        }

        internal Encoding OutputEncoding => OutputEncodingId switch
        {
            null or "" => new UTF8Encoding(false, false),
            _ => Encoding.GetEncoding(OutputEncodingId)
        };

        internal int LobFetchSizeB
        {
            get
            {
                if (LobFetchSize is null or "")
                {
                    return 262144;
                }
                else
                {
                    string lobFetchWoUnit = LobFetchSize[0..^1];
                    if (LobFetchSize.EndsWith("K", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt32(lobFetchWoUnit) * 1024;
                    else if (LobFetchSize.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024;
                    else if (LobFetchSize.EndsWith("G", StringComparison.OrdinalIgnoreCase))
                        return Convert.ToInt32(lobFetchWoUnit) * 1024 * 1024 * 1024;
                    else
                        throw new ArgumentOutOfRangeException(nameof(LobFetchSize), $"Unrecognized unit of LOB fetch size \"{LobFetchSize}\"");
                }
            }
        }

        internal InputSqlReturnType GetUltimateScriptType()
        {
            InputSqlReturnType result;
            if (InputSqlReturnTypeTable)
                result = InputSqlReturnType.Table;
            else if (InputSqlReturnTypeSelect)
                result = InputSqlReturnType.Select;
            else if (InputSqlReturnTypeCursor)
                result = InputSqlReturnType.RefCursor;
            else if (InputSqlReturnTypeMultiImplicit)
                result = InputSqlReturnType.MultiImplicitCursors;
            else
                throw new ArgumentOutOfRangeException("No input SQL return type specified");

            return result;
        }
    }
}
