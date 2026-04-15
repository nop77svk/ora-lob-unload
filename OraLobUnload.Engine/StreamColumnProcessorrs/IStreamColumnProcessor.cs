namespace NoP77svk.OraLobUnload.Engine.StreamColumnProcessors;

using System.IO;

public interface IStreamColumnProcessor
{
    long GetTrueLobLength(long reportedLength);

    string GetFormattedLobLength(long reportedLength);

    Task SaveLobToStreamAsync(Stream inLob, Stream outFile);
}
