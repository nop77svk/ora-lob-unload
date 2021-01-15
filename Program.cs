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

            var clobInputDecoder = new UnicodeEncoding(false, false);
            var clobOutputEncoder = new UTF8Encoding(false); // 2do! encoding as cmdln argument

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
                        SaveDataFromReader<OracleClob>(
                            dbReader,
                            (inStream) => new CryptoStream(inStream, new CharsetEncoderForClob(clobInputDecoder, clobOutputEncoder), CryptoStreamMode.Write, true),
                            (reader, fieldIndex) => reader.GetOracleClob(fieldIndex),
                            (lobContents, outStream) => { lobContents.CorrectlyCopyTo(outStream); },
                            (lobContents, fileName) => { Console.WriteLine($"Saving a {lobContents.Length / 2} characters long CLOB to \"{fileName}\""); }
                        );
                    }
                    else if (fieldTwoTypeName == "OracleBlob")
                    {
                        SaveDataFromReader<OracleBlob>(
                            dbReader,
                            (inStream) => new BufferedStream(inStream), // 2do! not really neccessary here; might be a reason for switching from delegate-driven generic method to a set of 3 specialization classes
                            (reader, fieldIndex) => reader.GetOracleBlob(fieldIndex),
                            (lobContents, outStream) => { lobContents.CopyTo(outStream); },
                            (lobContents, fileName) => { Console.WriteLine($"Saving a {lobContents.Length} bytes long BLOB to \"{fileName}\""); }
                        );
                    }
                    else if (fieldTwoTypeName == "OracleBFile")
                    {
                        SaveDataFromReader<OracleBFile>(
                            dbReader,
                            (inStream) => new BufferedStream(inStream), // 2do! not really neccessary here; might be a reason for switching from delegate-driven generic method to a set of 3 specialization classes
                            (reader, fieldIndex) => reader.GetOracleBFile(fieldIndex),
                            (lobContents, outStream) => { lobContents.CopyTo(outStream); },
                            (lobContents, fileName) => { Console.WriteLine($"Saving a {lobContents.Length} bytes long BFILE to \"{fileName}\""); }
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

        internal static void SaveDataFromReader<TOracleLob>(
            OracleDataReader dbReader,
            Func<Stream, Stream> createTransformStream,
            Func<OracleDataReader, int, TOracleLob> getLobStream,
            Action<TOracleLob, Stream> copyLobStream,
            Action<TOracleLob, string> copyStartFeedback
        )
        {
            while (dbReader.Read())
            {
                string fileName = dbReader.GetString(0);
                using Stream outFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                using Stream outTransformer = createTransformStream(outFile);

                TOracleLob lobContents = getLobStream(dbReader, 1);
                try
                {
                    copyStartFeedback(lobContents, fileName);
                    copyLobStream(lobContents, outTransformer);
                }
                finally
                {
                    if (lobContents is IDisposable disposableLob) // 2do! can this be enforced during compilation somehow?
                        disposableLob.Dispose();
                }
            }
        }
    }
}
