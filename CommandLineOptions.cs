namespace OraLobUnload
{
    using System.Collections.Generic;
    using CommandLine;

    internal class CommandLineOptions
    {
        [Option('f', "file", Required = false, HelpText = "Input SQL script file")]
        public string InputSqlScriptFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output folder")]
        public string OutputFolder { get; set; }

        [Option("return-type", Required = true, Default = "query", HelpText = "Input SQL script file content type; \"query\" or \"cursor\" or \"scalars\" or \"multi-implicit\"", SetName = "return-type")]
        public string InputSqlReturnTypeStr { get; set; }

        [Option('t', "return-type-table", Required = true, Default = false, SetName = "return-type-table")]
        public bool InputSqlReturnTypeTable { get; set; }

        [Option('q', "return-type-query", Required = true, Default = false, SetName = "return-type-query")]
        public bool InputSqlReturnTypeSelect { get; set; }

        [Option('c', "return-type-cursor", Required = true, Default = false, SetName = "return-type-cursor")]
        public bool InputSqlReturnTypeCursor { get; set; }

        [Option('s', "return-type-scalars", Required = true, Default = false, SetName = "return-type-scalars")]
        public bool InputSqlReturnTypeScalars { get; set; }

        [Option('m', "return-type-multi-implicit", Required = true, Default = false, SetName = "return-type-implicit")]
        public bool InputSqlReturnTypeMultiImplicit { get; set; }

        [Option('u', "db-user", Required = true)]
        public string DbUser { get; set; }

        [Option('p', "db-pasword", Required = true)]
        public string DbPassword { get; set; }

        [Option('d', "db", Required = true)]
        public string DbService { get; set; }

        [Option('v', "argument", Required = false, Separator = ',')]
        public IEnumerable<string> InputSqlArguments { get; set; }
    }
}
