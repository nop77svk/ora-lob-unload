namespace NoP77svk.OraLobUnload.Engine;
using System;

public class DataUnloaderException
    : Exception
{
    public string FileName { get; }

    public DataUnloaderException(string fileName)
        : base($"Error unloading LOB for file \"{fileName}\"")
    {
        FileName = fileName;
    }

    public DataUnloaderException(string fileName, string? message)
        : base(message)
    {
        FileName = fileName;
    }

    public DataUnloaderException(string fileName, Exception? innerException)
        : base($"Error unloading LOB for file \"{fileName}\"", innerException)
    {
        FileName = fileName;
    }
}
