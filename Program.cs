namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using CommandLine;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    internal static class Program
    {
        #pragma warning disable SA1500 // Braces for multi-line statements should not share line
        internal static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult<CommandLineOptions, int>(
                    options => MainWithOptions(options),
                    _ => {
                        Console.WriteLine("Something bad happened on the command line (2do!)");
                        return 255;
                    }
                );
        }
        #pragma warning restore SA1500 // Braces for multi-line statements should not share line

        internal static int MainWithOptions(CommandLineOptions options)
        {
            InputSqlReturnType scriptType = options.GetUltimateScriptType();

            using StreamReader inputSqlScriptReader = OpenInputSqlScript(options.InputSqlScriptFile);
            string inputSqlText = inputSqlScriptReader.ReadToEnd();

            using var dbConnection = new OracleConnection($"Data Source = {options.DbService}; User Id = {options.DbUser}; Password = {options.DbPassword}");
            dbConnection.Open();

            var dbCommandFactory = new InputSqlCommandFactory(dbConnection);
            IEnumerable<OracleCommand> dbCommandList = dbCommandFactory.CreateDbCommands(scriptType, inputSqlText, options.InputSqlArguments);

            var clobInputDecoder = new UnicodeEncoding(false, false);
            var clobOutputEncoder = new UTF8Encoding(false); // 2do! encoding as cmdln argument

            foreach (OracleCommand dbCommand in dbCommandList)
            {
                using (dbCommand)
                {
                    using OracleDataReader dbReader = dbCommand.ExecuteReader(System.Data.CommandBehavior.Default);
                    while (dbReader.Read())
                    {
                        if (dbReader.FieldCount != 2)
                            throw new InvalidDataException($"Dataset field count is {dbReader.FieldCount}, should be exactly 2");

                        string fieldOneTypeName = dbReader.GetFieldType(0).Name;
                        if (fieldOneTypeName != "string")
                            throw new InvalidDataException($"Field #1 is of type \"{fieldOneTypeName}\", but \"string\" expected");

                        string fieldTwoTypeName = dbReader.GetProviderSpecificFieldType(1).Name;

                        string fileName = dbReader.GetString(0);
                        using var outFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                        if (fieldTwoTypeName == "OracleClob")
                        {
                            using OracleClob lobContents = dbReader.GetOracleClob(1);
                            Console.WriteLine($"Saving {lobContents.Length / 2} characters long CLOB to {fileName}");
                            using var outFileRecoded = new CryptoStream(outFile, new CharsetEncoderForClob(clobInputDecoder, clobOutputEncoder), CryptoStreamMode.Write, true);
                            lobContents.CorrectlyCopyTo(outFileRecoded);
                        }
                        else if (fieldTwoTypeName == "OracleBlob")
                        {
                            using OracleBlob lobContents = dbReader.GetOracleBlob(1);
                            Console.WriteLine($"Saving {lobContents.Length} bytes long BLOB to {fileName}");
                            lobContents.CopyTo(outFile);
                        }
                        else if (fieldTwoTypeName == "OracleBFile")
                        {
                            using OracleBFile lobContents = dbReader.GetOracleBFile(1);
                            Console.WriteLine($"Saving {lobContents.Length} bytes long BFILE to {fileName}");
                            lobContents.CopyTo(outFile);
                        }
                        else
                        {
                            throw new InvalidDataException($"Field #2 is of type \"{fieldTwoTypeName}\", but LOB or BFILE expected");
                        }
                    }
                }
            }

            return 0;
        }

        internal static StreamReader OpenInputSqlScript(string inputSqlScriptFile)
        {
            return inputSqlScriptFile switch
            {
                "" => new StreamReader(Console.OpenStandardInput()),
                _ => File.OpenText(inputSqlScriptFile)
            };
        }
    }
}
