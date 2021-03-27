namespace SK.NoP77svk.OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    internal class OracleConnectStringParsed
    {
        internal OracleConnectStringParsed()
        {
            User = "";
            Password = "";
            DbService = "";
            SpecialRole = OracleUserConnectRole.Normal;
        }

        internal OracleConnectStringParsed(string connectString)
        {
            (User, Password, DbService, SpecialRole) = InternalParseConnectString(connectString);
        }

        internal string User { get; set; }

        internal string Password { get; set; }

        internal string DbService { get; set; }

        internal OracleUserConnectRole SpecialRole { get; set; }

        internal string FullConnectString
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
                (User, Password, DbService, SpecialRole) = InternalParseConnectString(value);
            }
        }

        public override string ToString()
        {
            return FullConnectString;
        }

        private static ValueTuple<string, string, string, OracleUserConnectRole> InternalParseConnectString(string value)
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
}
