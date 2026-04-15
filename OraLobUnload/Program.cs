namespace NoP77svk.OraLobUnload;

using System;
using System.IO;

using CommandLine;

using NoP77svk.OraLobUnload.Engine;
using NoP77svk.OraLobUnload.Engine.Database;
using NoP77svk.OraLobUnload.Engine.DataReaders;
using NoP77svk.OraLobUnload.Engine.Infrastructure;
using NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using Oracle.ManagedDataAccess.Client;

internal static class Program
{
    internal static async Task<int> Main(string[] args)
    {
        await Parser
            .Default
            .ParseArguments<CliOptions>(args)
            .WithParsedAsync(async opt => await MainWithOptions(opt));

        return 0;
    }

    internal static async Task MainWithOptions(CliOptions options)
    {
        await Console.Error.WriteLineAsync("Oracle LOB Unloader");
        await Console.Error.WriteLineAsync($"by Peter Hraško a.k.a NoP77svk");
        await Console.Error.WriteLineAsync($"https://github.com/NoP77svk/ora-lob-unload");
        await Console.Error.WriteLineAsync();

        ValidateCommandLineArguments(options);

        using StreamReader inputSqlScriptReader = OpenInputSqlScript(options.InputFile);

        await Console.Error.WriteLineAsync($"Connecting to {options.DbLogon.DisplayableConnectString}");
        await using OracleConnection dbConnection = OracleConnectionFactory.CreateOracleConnection(options.DbLogon.DbService, options.DbLogon.User, options.DbLogon.Password);
        await dbConnection.OpenAsync();

        await Console.Error.WriteLineAsync($"Using {InputScriptFactory.GetInputSqlReturnTypeDesc(options.InputFileContentType)} as an input against the database");
        InputScriptFactory inputScriptFactory = new InputScriptFactory(dbConnection)
        {
            InitialLobFetchSize = options.GetLobInitFetchSizeB()
        };

        IDataMultiReader dataMultiReader = inputScriptFactory.CreateMultiReader(options.InputFileContentType, inputSqlScriptReader);

        if (!string.IsNullOrEmpty(options.OutputFolder))
        {
            await Console.Error.WriteLineAsync($"Output folder: {options.OutputFolder}");
        }
        else
        {
            await Console.Error.WriteLineAsync("Output folder: (current)");
        }

        await Console.Error.WriteLineAsync($"Output CLOBs encoding: {options.OutputEncoding.HeaderName}");

        DataUnloader unloader = new DataUnloader()
        {
            OutputPath = options.OutputFolder,
            OutputFileExtension = options.OutputFileExtension,
            VisualFeedbackStartUnloading = (fName, lobLen) => { Console.Error.Write($"{fName} [{lobLen}] ..."); },
            VisualFeedbackFinish = () => { Console.Error.WriteLine(" done"); }
        };

        await foreach (DataMultiReaderRow row in dataMultiReader.GetDataAsync(options.FileNameColumnIndex - 1, options.LobColumnIndex - 1))
        {
            IStreamColumnProcessor processor = StreamColumnProcessorFactory.CreateStreamColumnProcessor(row.LobContents?.GetType(), options.OutputEncoding);
            await unloader.UnloadDataFromMultiReaderAsync(row.LobName, row.LobContents, processor);
        }

        await Console.Error.WriteLineAsync("DONE");
    }

    internal static StreamReader OpenInputSqlScript(string? inputSqlScriptFile)
    {
        if (string.IsNullOrEmpty(inputSqlScriptFile))
        {
            return new StreamReader(Console.OpenStandardInput());
        }
        else
        {
            return File.OpenText(inputSqlScriptFile);
        }
    }

    internal static void ValidateCommandLineArguments(CliOptions options)
    {
        if (string.IsNullOrEmpty(options.DbLogon.DbService))
        {
            throw new CliOptionException("Database service name not supplied");
        }

        if (string.IsNullOrEmpty(options.DbLogon.User))
        {
            throw new CliOptionException("Connecting database user not supplied");
        }

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
        {
            throw new CliOptionException("Connecting database user's password not supplied");
        }

        if (options.FileNameColumnIndex is < 1 or > 1000)
        {
            throw new CliOptionException("File name column index must be between 1 and 1000 (inclusive)");
        }

        if (options.LobColumnIndex is < 1 or > 1000)
        {
            throw new CliOptionException("LOB column index must be between 1 and 1000 (inclusive)");
        }

        if (options.LobColumnIndex == options.FileNameColumnIndex)
        {
            throw new CliOptionException($"LOB column index {options.LobColumnIndex} cannot be the same as file name column index {options.FileNameColumnIndex}");
        }
    }
}
