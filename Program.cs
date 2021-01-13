namespace OraLobUnload
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using CommandLine;
    using Oracle.ManagedDataAccess.Client;
    using Oracle.ManagedDataAccess.Types;

    static class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CommandLineOptions>(args)
                .MapResult<CommandLineOptions, int>(
                    options => MainWithOptions(options),
                    _ => { Console.WriteLine("Something bad happened on the command line (2do!)"); return 255; }
                );
        }

        static int MainWithOptions(CommandLineOptions options)
        {
            var scriptType = InputSqlReturnTypeEnumHelpers.GetUltimateScriptType(options);

            using var inputSqlScriptReader = OpenInputSqlScript(options.InputSqlScriptFile);
            var inputSqlText = inputSqlScriptReader.ReadToEnd();

            using var dbConnection = new OracleConnection($"Data Source = {options.DbService}; User Id = {options.DbUser}; Password = {options.DbPassword}");
            dbConnection.Open();

            var dbCommandFactory = new InputSqlCommandFactory(dbConnection);
            var dbReaderList = dbCommandFactory.GetResultReaders(scriptType, inputSqlText, options.InputSqlArguments);

            string fileName;
            foreach (OracleDataReader dbReader in dbReaderList)
            {
                while (dbReader.Read())
                {
                    fileName = dbReader.GetString(0);
                    Console.WriteLine($"File name = \"{fileName}\"");
                    using var lobContents = dbReader.GetOracleClob(1);
                    using var outFile = File.Create(fileName);
                    lobContents.CopyTo(outFile);
                    outFile.Close();
                    lobContents.Close();
                }
                dbReader.Close();
            }

            return 0;
        }


        static StreamReader OpenInputSqlScript(string inputSqlScriptFile)
        {
            return inputSqlScriptFile switch
            {
                "" => new StreamReader(Console.OpenStandardInput()),
                _ => File.OpenText(inputSqlScriptFile)
            };
        }
    }
}
