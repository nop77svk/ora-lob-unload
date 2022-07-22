namespace NoP77svk.OraLobUnload.OracleStuff;
using System;
using Oracle.ManagedDataAccess.Client;

public static class OracleConnectionFactory
{
    public static OracleConnection CreateOracleConnection(string? dbService, string? dbUser, string? dbPassword)
    {
        if (dbService is null or "")
            throw new ArgumentNullException(nameof(dbService));
        if (dbUser is null or "")
            throw new ArgumentNullException(nameof(dbUser));
        if (dbPassword is null or "")
            throw new ArgumentNullException(nameof(dbPassword));

        return new OracleConnection($"Data Source = {dbService}; User Id = {dbUser}; Password = {dbPassword}");
    }
}
