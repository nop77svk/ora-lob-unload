namespace NoP77svk.OraLobUnload;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using NoP77svk.OraLobUnload.DataReaders;
using NoP77svk.OraLobUnload.StreamColumnProcessors;
using NoP77svk.OraLobUnload.Utilities;
using Oracle.ManagedDataAccess.Client;

internal static class Program
{
    private static readonly HashSet<string> _foldersCreated = new ();

    internal static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<CommandLineOptions>(args)
            .MapResult<CommandLineOptions, int>(
                options => MainWithOptions(options),
                _ => 255
            );
    }

    internal static int MainWithOptions(CommandLineOptions options)
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
        using var dbConnection = OracleConnectionFactory(options.DbLogon.DbService, options.DbLogon.User, options.DbLogon.Password);
        dbConnection.Open();

        Console.Error.WriteLine($"Using {InputScriptFactory.GetInputSqlReturnTypeDesc(options.InputFileContentType)} as an input against the database");
        var dbCommandFactory = new InputScriptFactory(dbConnection, options.LobInitFetchSizeB);
        using IDataMultiReader dataMultiReader = dbCommandFactory.CreateMultiReader(options.InputFileContentType, inputSqlScriptReader);

        if (options.OutputFolder is not null and not "")
            Console.Error.WriteLine($"Output folder: {options.OutputFolder}");
        else
            Console.Error.WriteLine("Output folder: (current)");

        Console.Error.WriteLine($"Output CLOBs encoding: {options.OutputEncoding.HeaderName}");

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

                using IStreamColumnProcessor processor = StreamColumnProcessorFactory(
                    dbReader.GetProviderSpecificFieldType(options.LobColumnIndex - 1),
                    $"# {options.LobColumnIndex - 1} ({dbReader.GetName(options.LobColumnIndex - 1)})",
                    options.OutputEncoding
                );
                SaveDataFromReader(
                    dbReader,
                    options.FileNameColumnIndex - 1,
                    options.LobColumnIndex - 1,
                    processor,
                    options.OutputFileExtension,
                    options.OutputFolder
                );
            }
        }

        Console.Error.WriteLine("DONE");
        return 0;
    }

    internal static void CreateFilePath(string? filePath)
    {
        if (filePath is not null and not "")
        {
            if (!_foldersCreated.Contains(filePath))
            {
                Directory.CreateDirectory(filePath);
                _foldersCreated.Add(filePath);
            }
        }
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

    internal static void SaveDataFromReader(OracleDataReader dataReader, int fileNameColumnIx, int lobColumnIx, IStreamColumnProcessor processor, string? fileNameExt, string? outputPath)
    {
        string cleanedFileNameExt = fileNameExt is not null and not "" ? "." + fileNameExt.Trim('.') : "";
        while (dataReader.Read())
        {
            string fileName = dataReader.GetString(fileNameColumnIx);
            fileName = Path.Combine(outputPath ?? "", fileName);
            string fileNameWithExt = cleanedFileNameExt != "" && !fileName.EndsWith(cleanedFileNameExt, StringComparison.OrdinalIgnoreCase)
                ? fileName + cleanedFileNameExt
                : fileName;

            CreateFilePath(Path.GetDirectoryName(fileNameWithExt));
            using Stream outFile = new FileStream(fileNameWithExt, FileMode.Create, FileAccess.Write);

            using Stream lobContents = processor.ReadLob(dataReader, lobColumnIx);
            Console.Error.Write($"{fileNameWithExt} [{processor.GetFormattedLobLength(lobContents.Length)}] ...");
            processor.SaveLobToStream(lobContents, outFile);
            Console.Error.WriteLine(" done");
        }
    }

    internal static IStreamColumnProcessor StreamColumnProcessorFactory(Type columnType, string columnDescription, Encoding charColumnOutputEncoding)
    {
        return columnType.Name switch
        {
            "OracleClob" => new ClobProcessor(charColumnOutputEncoding),
            "OracleBlob" => new BlobProcessor(),
            "OracleBFile" => new BFileProcessor(),
            _ => throw new InvalidDataException($"Supposed LOB column {columnDescription} is of type \"{columnType.Name}\", but CLOB, BLOB or BFILE expected")
        };
    }

    internal static void ValidateCommandLineArguments(CommandLineOptions options)
    {
        if (options.DbLogon.DbService is null or "")
            throw new ArgumentNullException(null, "Database service name not supplied");
        if (options.DbLogon.User is null or "")
            throw new ArgumentNullException(null, "Connecting database user not supplied");

        if (options.DbLogon.Password is null or "")
        {
            Console.Error.Write($"Enter password for {options.DbLogon.DisplayableConnectString}: ");
            Random charRandomizer = new Random();
            SecretSystemConsole secretConsole = new SecretSystemConsole(x => Convert.ToChar(charRandomizer.Next(32, 127)))
            {
                CancelOnEscape = true
            };
            options.DbLogon.Password = secretConsole.ReadLineInSecret();
        }

        if (options.DbLogon.Password is null or "")
            throw new ArgumentNullException(null, "Connecting database user's password not supplied");

        if (options.FileNameColumnIndex is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(null, "File name column index must be between 1 and 1000 (inclusive)");
        if (options.LobColumnIndex is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(null, "LOB column index must be between 1 and 1000 (inclusive)");
        if (options.LobColumnIndex == options.FileNameColumnIndex)
            throw new ArgumentException($"LOB column index {options.LobColumnIndex} cannot be the same as file name column index {options.FileNameColumnIndex}");
    }
}
