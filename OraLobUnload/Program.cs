namespace NoP77svk.OraLobUnload;

using System;
using System.IO;
using CommandLine;
using NoP77svk.OraLobUnload.DataReaders;
using NoP77svk.OraLobUnload.Engine;
using NoP77svk.OraLobUnload.OracleStuff;
using NoP77svk.OraLobUnload.StreamColumnProcessors;
using NoP77svk.OraLobUnload.Utilities;
using Oracle.ManagedDataAccess.Client;

internal static class Program
{
    internal static int Main(string[] args)
    {
        return Parser.Default
            .ParseArguments<CLI>(args)
            .MapResult(
                options => MainWithOptions(options),
                _ => 255
            );
    }

    internal static int MainWithOptions(CLI options)
    {
        Console.Error.WriteLine("Oracle LOB Unloader");
        Console.Error.WriteLine($"by Peter Hraško a.k.a NoP77svk");
        Console.Error.WriteLine($"https://github.com/NoP77svk/ora-lob-unload");
        Console.Error.WriteLine();

        try
        {
            ValidateCommandLineArguments(options);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"ERROR: {e.Message}");
            return 254;
        }

        using StreamReader inputSqlScriptReader = OpenInputSqlScript(options.InputFile);

        Console.Error.WriteLine($"Connecting to {options.DbLogon.DisplayableConnectString}");
        using OracleConnection dbConnection = OracleConnectionFactory.CreateOracleConnection(options.DbLogon.DbService, options.DbLogon.User, options.DbLogon.Password);
        dbConnection.Open();

        Console.Error.WriteLine($"Using {InputScriptFactory.GetInputSqlReturnTypeDesc(options.InputFileContentType)} as an input against the database");
        InputScriptFactory inputScriptFactory = new InputScriptFactory(dbConnection)
        {
            InitialLobFetchSize = options.LobInitFetchSizeB
        };
        using IDataMultiReader dataMultiReader = inputScriptFactory.CreateMultiReader(options.InputFileContentType, inputSqlScriptReader);

        if (!string.IsNullOrEmpty(options.OutputFolder))
            Console.Error.WriteLine($"Output folder: {options.OutputFolder}");
        else
            Console.Error.WriteLine("Output folder: (current)");

        Console.Error.WriteLine($"Output CLOBs encoding: {options.OutputEncoding.HeaderName}");

        DataUnloader unloader = new DataUnloader()
        {
            FileNameColumnIndex = options.FileNameColumnIndex,
            LobColumnIndex = options.LobColumnIndex,
            OutputPath = options.OutputFolder,
            OutputFileExtension = options.OutputFileExtension,
            VisualFeedbackStartUnloading = (fName, lobLen) => { Console.Error.Write($"{fName} [{lobLen}] ..."); },
            VisualFeedbackFinish = () => { Console.Error.WriteLine(" done"); }
        };

        foreach (OracleDataReader dbReader in dataMultiReader.CreateDataReaders())
        {
            using (dbReader)
            {
                int leastDatasetColumnCountNeeded = Math.Max(options.FileNameColumnIndex, options.LobColumnIndex);
                if (dbReader.FieldCount < leastDatasetColumnCountNeeded)
                    throw new InvalidDataException($"Dataset field count is {dbReader.FieldCount}, should be at least {leastDatasetColumnCountNeeded}");

                string fileNameColumnTypeName = dbReader.GetFieldType(options.FileNameColumnIndex - 1).Name;
                if (fileNameColumnTypeName != "String")
                    throw new InvalidDataException($"Supposed file name column #{options.FileNameColumnIndex} is of type \"{fileNameColumnTypeName}\", but \"string\" expected");

                using IStreamColumnProcessor processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor(
                    dbReader.GetProviderSpecificFieldType(options.LobColumnIndex - 1),
                    options.OutputEncoding
                );

                unloader.UnloadDataFromReader(dbReader, processor);
            }
        }

        Console.Error.WriteLine("DONE");
        return 0;
    }

    internal static StreamReader OpenInputSqlScript(string? inputSqlScriptFile)
    {
        if (string.IsNullOrEmpty(inputSqlScriptFile))
            return new StreamReader(Console.OpenStandardInput());
        else
            return File.OpenText(inputSqlScriptFile);
    }

    internal static void ValidateCommandLineArguments(CLI options)
    {
        if (string.IsNullOrEmpty(options.DbLogon.DbService))
            throw new ArgumentNullException(null, "Database service name not supplied");
        if (string.IsNullOrEmpty(options.DbLogon.User))
            throw new ArgumentNullException(null, "Connecting database user not supplied");

        if (string.IsNullOrEmpty(options.DbLogon.Password))
        {
            Console.Error.Write($"Enter password for {options.DbLogon.DisplayableConnectString}: ");
            Random charRandomizer = new Random();
            SecretSystemConsole secretConsole = new SecretSystemConsole(x => Convert.ToChar(charRandomizer.Next(32, 127)))
            {
                CancelOnEscape = true
            };
            options.DbLogon.Password = secretConsole.ReadLineInSecret();
        }

        if (string.IsNullOrEmpty(options.DbLogon.Password))
            throw new ArgumentNullException(null, "Connecting database user's password not supplied");

        if (options.FileNameColumnIndex is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(null, "File name column index must be between 1 and 1000 (inclusive)");
        if (options.LobColumnIndex is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(null, "LOB column index must be between 1 and 1000 (inclusive)");
        if (options.LobColumnIndex == options.FileNameColumnIndex)
            throw new ArgumentException($"LOB column index {options.LobColumnIndex} cannot be the same as file name column index {options.FileNameColumnIndex}");
    }
}
