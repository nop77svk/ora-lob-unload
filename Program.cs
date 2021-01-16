namespace OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using CommandLine;
    using Oracle.ManagedDataAccess.Client;
    using OraLobUnload.DatasetProcessors;

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
                    });
        }
        #pragma warning restore SA1500 // Braces for multi-line statements should not share line

        internal static int MainWithOptions(CommandLineOptions options)
        {
            InputSqlReturnType scriptType = options.GetUltimateScriptType();

            using StreamReader inputSqlScriptReader = OpenInputSqlScript(options.InputSqlScriptFile);

            if (options.DbService is null or "")
                throw new ArgumentNullException(nameof(options.DbService));
            if (options.DbUser is null or "")
                throw new ArgumentNullException(nameof(options.DbUser));
            if (options.DbPassword is null or "")
                throw new ArgumentNullException(nameof(options.DbPassword));

            using var dbConnection = new OracleConnection($"Data Source = {options.DbService}; User Id = {options.DbUser}; Password = {options.DbPassword}");
            dbConnection.Open();

            var dbCommandFactory = new InputSqlCommandFactory(dbConnection);
            IEnumerable<OracleCommand> dbCommandList = dbCommandFactory.CreateDbCommands(scriptType, inputSqlScriptReader, options.InputSqlArguments);

            foreach (OracleCommand dbCommand in dbCommandList)
            {
                using (dbCommand)
                {
                    using OracleDataReader dbReader = dbCommand.ExecuteReader(System.Data.CommandBehavior.Default);
                    if (dbReader.FieldCount != 2)
                        throw new InvalidDataException($"Dataset field count is {dbReader.FieldCount}, should be exactly 2");

                    string fieldOneTypeName = dbReader.GetFieldType(0).Name;
                    if (fieldOneTypeName != "String")
                        throw new InvalidDataException($"Field #1 is of type \"{fieldOneTypeName}\", but \"string\" expected");

                    string fieldTwoTypeName = dbReader.GetProviderSpecificFieldType(1).Name;

                    if (fieldTwoTypeName == "OracleClob")
                    {
                        SaveDataFromReader(
                            dbReader,
                            new ClobProcessor(options.OutputEncoding),
                            (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} characters long CLOB to \"{fileName}\" with encoding of "); }
                        );
                    }
                    else if (fieldTwoTypeName == "OracleBlob")
                    {
                        SaveDataFromReader(
                            dbReader,
                            new BlobProcessor(),
                            (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} bytes long BLOB to \"{fileName}\""); }
                        );
                    }
                    else if (fieldTwoTypeName == "OracleBFile")
                    {
                        SaveDataFromReader(
                            dbReader,
                            new BFileProcessor(),
                            (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} bytes long BFILE to \"{fileName}\""); }
                        );
                    }
                    else
                    {
                        throw new InvalidDataException($"Field #2 is of type \"{fieldTwoTypeName}\", but LOB or BFILE expected");
                    }
                }
            }

            return 0;
        }

        internal static StreamReader OpenInputSqlScript(string? inputSqlScriptFile)
        {
            return inputSqlScriptFile switch
            {
                "" or null => new StreamReader(Console.OpenStandardInput()),
                _ => File.OpenText(inputSqlScriptFile)
            };
        }

        internal static void SaveDataFromReader(OracleDataReader reader, IDataReaderToStream processor, Action<long, string> copyStartFeedback)
        {
            while (reader.Read())
            {
                string fileName = reader.GetString(0);
                using Stream outFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                using Stream lobContents = processor.ReadLob(reader, 1);
                copyStartFeedback(processor.GetTrueLobLength(lobContents.Length), fileName);
                processor.SaveLobToStream(lobContents, outFile);
            }
        }
    }
}
