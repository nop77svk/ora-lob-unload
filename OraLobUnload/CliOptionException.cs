namespace NoP77svk.OraLobUnload;

using System;

public class CliOptionException : Exception
{
    public CliOptionException()
    {
    }

    public CliOptionException(string message)
        : base(message)
    {
    }

    public CliOptionException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
