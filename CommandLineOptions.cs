namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using CommandLine;

    internal class CommandLineOptions
    {
        [Option('f', "file", Required = false, HelpText = "Input SQL script file")]
        public string? InputSqlScriptFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output folder")]
        public string? OutputFolder { get; set; }

        [Option('x', "output-file-extension", Required = false)]
        public string? OutputFileExtension { get; set; }

        [Option("clob-output-charset", Required = false, Default = "utf-8")]
        public string? OutputEncodingId { get; set; }

        [Option('t', "use-table", Required = true, Default = false, SetName = "in-type-table")]
        public bool InputSqlReturnTypeTable { get; set; }

        [Option("file-name-column-ix", Required = false, Default = 1, SetName = "in-type-table")]
        public int FileNameColumnIndex { get; set; }

        [Option("lob-column-ix", Required = false, Default = 2, SetName = "in-type-table")]
        public int LobColumnIndex { get; set; }

        [Option('q', "use-query", Required = true, Default = false, SetName = "in-type-query")]
        public bool InputSqlReturnTypeSelect { get; set; }

        [Option('c', "use-cursor", Required = true, Default = false, SetName = "in-type-cursor")]
        public bool InputSqlReturnTypeCursor { get; set; }

        [Option('s', "use-scalars", Required = true, Default = false, SetName = "in-type-scalars")]
        public bool InputSqlReturnTypeScalars { get; set; }

        [Option('m', "use-implicit-cursor", Required = true, Default = false, SetName = "in-type-implicit")]
        public bool InputSqlReturnTypeMultiImplicit { get; set; }

        [Option('u', "db-user", Required = true)]
        public string? DbUser { get; set; }

        [Option('p', "db-pasword", Required = true)]
        public string? DbPassword { get; set; }

        [Option('d', "db", Required = true)]
        public string? DbService { get; set; }

        [Option('v', "argument", Required = false, Separator = ',')]
        public IEnumerable<string>? InputSqlArguments { get; set; }

        internal Encoding OutputEncoding => OutputEncodingId switch
        {
            null or "" => new UTF8Encoding(false, false),
            _ => Encoding.GetEncoding(OutputEncodingId)
        };

        internal InputSqlReturnType GetUltimateScriptType()
        {
            InputSqlReturnType result;
            if (InputSqlReturnTypeTable)
                result = InputSqlReturnType.Table;
            else if (InputSqlReturnTypeSelect)
                result = InputSqlReturnType.Select;
            else if (InputSqlReturnTypeScalars)
                result = InputSqlReturnType.Scalars;
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
