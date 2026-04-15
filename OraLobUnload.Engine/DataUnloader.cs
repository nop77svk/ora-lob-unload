namespace NoP77svk.OraLobUnload.Engine;

using System;
using System.Collections.Generic;
using System.IO;

using NoP77svk.OraLobUnload.DataReaders;
using NoP77svk.OraLobUnload.StreamColumnProcessors;

using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

public class DataUnloader
{
    public int FileNameColumnIndex { get; init; } = 1;
    public int LobColumnIndex { get; init; } = 2;
    public string? OutputPath { get; init; } = null;
    public string? OutputFileExtension { get; init; } = null;

    public Action<string, string>? VisualFeedbackStartUnloading { get; init; }
    public Action? VisualFeedbackFinish { get; init; }

    private static readonly HashSet<string> _foldersCreated = new();

    private string CleanedFileNameExt
        => !string.IsNullOrEmpty(OutputFileExtension)
        ? "." + OutputFileExtension.Trim('.')
        : string.Empty;

    public async Task UnloadDataFromMultiReaderAsync(string fileName, Stream? fileContents, IStreamColumnProcessor processor)
    {
        string cleanedFileNameExt = CleanedFileNameExt;
        try
        {
            string fileNameWithPath = Path.Combine(OutputPath ?? string.Empty, fileName);
            string fileNameWithExt = !string.IsNullOrEmpty(cleanedFileNameExt) && !fileName.EndsWith(cleanedFileNameExt, StringComparison.OrdinalIgnoreCase)
                ? fileNameWithPath + cleanedFileNameExt
                : fileNameWithPath;

            CreateFilePath(Path.GetDirectoryName(fileNameWithExt));
            await using Stream outFile = new FileStream(fileNameWithExt, FileMode.Create, FileAccess.Write);

            if (fileContents is not null)
            {
                await using Stream lobContents = fileContents;

                VisualFeedbackStartUnloading?.Invoke(fileNameWithExt, processor.GetFormattedLobLength(lobContents.Length));

                processor.SaveLobToStreamAsync(lobContents, outFile);
                VisualFeedbackFinish?.Invoke();

                lobContents.Close();
            }

            outFile.Close();
        }
        catch (OracleException ex)
        {
            throw new DataUnloaderException(fileName, ex);
        }
    }

    private static void CreateFilePath(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath) && !_foldersCreated.Contains(filePath))
        {
            Directory.CreateDirectory(filePath);
            _foldersCreated.Add(filePath);
        }
    }
}
