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
            ValidateOptions(options);

            using StreamReader inputSqlScriptReader = OpenInputSqlScript(options.InputSqlScriptFile);

            using var dbConnection = OracleConnectionFactory(options.DbService, options.DbUser, options.DbPassword);
            dbConnection.Open();

            var dbCommandFactory = new InputSqlCommandFactory(dbConnection);
            IEnumerable<OracleCommand> dbCommandList = dbCommandFactory.CreateDbCommands(options.GetUltimateScriptType(), inputSqlScriptReader, options.InputSqlArguments);

            foreach (OracleCommand dbCommand in dbCommandList)
            {
                using (dbCommand)
                {
                    using OracleDataReader dbReader = dbCommand.ExecuteReader(System.Data.CommandBehavior.Default);

                    int minimalDatasetColumnCount = Math.Max(options.FileNameColumnIndex, options.LobColumnIndex);
                    if (dbReader.FieldCount < minimalDatasetColumnCount)
                        throw new InvalidDataException($"Dataset field count is {dbReader.FieldCount}, should be at least {minimalDatasetColumnCount}");

                    string fileNameColumnTypeName = dbReader.GetFieldType(options.FileNameColumnIndex - 1).Name;
                    if (fileNameColumnTypeName != "String")
                        throw new InvalidDataException($"Supposed file name column #{options.FileNameColumnIndex} is of type \"{fileNameColumnTypeName}\", but \"string\" expected");

                    string lobColumnTypeName = dbReader.GetProviderSpecificFieldType(options.LobColumnIndex - 1).Name;
                    switch (lobColumnTypeName)
                    {
                        case "OracleClob":
                            SaveDataFromReader(
                                dbReader,
                                new ClobProcessor(options.OutputEncoding),
                                (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} characters long CLOB to \"{fileName}\" with encoding of "); }
                            );
                            break;
                        case "OracleBlob":
                            SaveDataFromReader(
                                dbReader,
                                new BlobProcessor(),
                                (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} bytes long BLOB to \"{fileName}\""); }
                            );
                            break;
                        case "OracleBFile":
                            SaveDataFromReader(
                                dbReader,
                                new BFileProcessor(),
                                (lobLength, fileName) => { Console.WriteLine($"Saving a {lobLength} bytes long BFILE to \"{fileName}\""); }
                            );
                            break;
                        default:
                            throw new InvalidDataException($"Supposed LOB column #{options.LobColumnIndex} is of type \"{lobColumnTypeName}\", but LOB or BFILE expected");
                    }
                }
            }

            return 0;
        }

        internal static void ValidateOptions(CommandLineOptions options)
        {
            if (options.FileNameColumnIndex is < 1 or > 1000)
                throw new ArgumentOutOfRangeException(nameof(options.FileNameColumnIndex), "Must be between 1 and 1000 (inclusive)");
            if (options.LobColumnIndex is < 1 or > 1000)
                throw new ArgumentOutOfRangeException(nameof(options.LobColumnIndex), "Must be between 1 and 1000 (inclusive)");
            if (options.LobColumnIndex == options.FileNameColumnIndex)
                throw new ArgumentException($"LOB column index {options.LobColumnIndex} cannot be the same as file name column index {options.FileNameColumnIndex}");

            if (options.DbService is null or "" || options.DbUser is null or "" || options.DbPassword is null or "")
                throw new ArgumentNullException("options.Db*", "Empty or incomplete database credentials supplied");
        }

        internal static StreamReader OpenInputSqlScript(string? inputSqlScriptFile)
        {
            return inputSqlScriptFile switch
            {
                "" or null => new StreamReader(Console.OpenStandardInput()),
                _ => File.OpenText(inputSqlScriptFile)
            };
        }

        internal static OracleConnection OracleConnectionFactory(string? dbService, string? dbUser, string? dbPassword)
        {
            if (dbService is null or "")
                throw new ArgumentNullException(nameof(dbService));
            if (dbUser is null or "")
                throw new ArgumentNullException(nameof(dbUser));
            if (dbPassword is null or "")
                throw new ArgumentNullException(nameof(dbPassword));

            return new OracleConnection($"Data Source = {dbService}; User Id = {dbUser}; Password = {dbPassword}");
        }

        internal static void SaveDataFromReader(OracleDataReader reader, IDataReaderToStream processor, Action<long, string>? copyStartFeedback)
        {
            while (reader.Read())
            {
                string fileName = reader.GetString(0);
                using Stream outFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);

                using Stream lobContents = processor.ReadLob(reader, 1);
                copyStartFeedback?.Invoke(processor.GetTrueLobLength(lobContents.Length), fileName);
                processor.SaveLobToStream(lobContents, outFile);
            }
        }
    }
}
