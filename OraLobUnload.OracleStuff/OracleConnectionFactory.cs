namespace NoP77svk.OraLobUnload.OracleStuff;
using System;
using Oracle.ManagedDataAccess.Client;

public static class OracleConnectionFactory
{
    public static OracleConnection CreateOracleConnection(string? dbService, string? dbUser, string? dbPassword)
    {
        if (string.IsNullOrEmpty(dbService))
            throw new ArgumentNullException(nameof(dbService));
        if (string.IsNullOrEmpty(dbUser))
            throw new ArgumentNullException(nameof(dbUser));
        if (string.IsNullOrEmpty(dbPassword))
            throw new ArgumentNullException(nameof(dbPassword));

        return new OracleConnection($"Data Source = {dbService}; User Id = {dbUser}; Password = {dbPassword}");
    }
}
