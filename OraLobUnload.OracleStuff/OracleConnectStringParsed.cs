namespace NoP77svk.OraLobUnload.OracleStuff;

using System;
using System.Text.RegularExpressions;

public class OracleConnectStringParsed
{
    public OracleConnectStringParsed()
    {
        User = "";
        Password = "";
        DbService = "";
        SpecialRole = OracleUserConnectRole.Normal;
    }

    public OracleConnectStringParsed(string connectString)
    {
        (User, Password, DbService, SpecialRole) = publicParseConnectString(connectString);
    }

    public string User { get; set; }

    public string Password { get; set; }

    public string DbService { get; set; }

    public OracleUserConnectRole SpecialRole { get; set; }

    public string FullConnectString
    {
        get => (User != "" ? User : "")
            + (User != "" && Password != "" ? "/" + Password : "")
            + (DbService != "" ? "@" + DbService : "")
            + SpecialRole switch
            {
                OracleUserConnectRole.AsSysDba => " as sysdba",
                OracleUserConnectRole.AsSysOper => " as sysoper",
                _ => ""
            };
        set
        {
            (User, Password, DbService, SpecialRole) = publicParseConnectString(value);
        }
    }

    public string DisplayableConnectString
    {
        get => (User != "" ? User : "")
            + (DbService != "" ? "@" + DbService : "")
            + SpecialRole switch
            {
                OracleUserConnectRole.AsSysDba => " as sysdba",
                OracleUserConnectRole.AsSysOper => " as sysoper",
                _ => ""
            };
    }

    public override string ToString()
    {
        return DisplayableConnectString;
    }

    private static ValueTuple<string, string, string, OracleUserConnectRole> publicParseConnectString(string value)
    {
        ValueTuple<string, string, string, OracleUserConnectRole> result;

        if (value is null or "")
        {
            result.Item1 = "";
            result.Item2 = "";
            result.Item3 = "";
            result.Item4 = OracleUserConnectRole.Normal;
        }
        else
        {
            Match m = Regex.Match(value, @"^\s*([^/@ ]*)(\s*/\s*([^@ ]*))?\s*@\s*(\S*)\s*(as\s*(sysdba|sysoper))?\s*$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                result.Item1 = m.Groups[1].Value;
                result.Item2 = m.Groups[3].Value;
                result.Item3 = m.Groups[4].Value;
                result.Item4 = m.Groups[6].Value.ToLower() switch
                {
                    "sysdba" => OracleUserConnectRole.AsSysDba,
                    "sysoper" => OracleUserConnectRole.AsSysOper,
                    _ => OracleUserConnectRole.Normal
                };
            }
            else
            {
                throw new ArgumentException($"\"{value}\" is not a valid Oracle connection string");
            }
        }

        return result;
    }
}
