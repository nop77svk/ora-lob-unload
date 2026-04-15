namespace NoP77svk.OraLobUnload.StreamColumnProcessors;

using System.IO;

public interface IStreamColumnProcessor
{
    long GetTrueLobLength(long reportedLength);

    string GetFormattedLobLength(long reportedLength);

    void SaveLobToStream(Stream inLob, Stream outFile);
}
