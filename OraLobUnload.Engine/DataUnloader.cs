namespace NoP77svk.OraLobUnload.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using NoP77svk.OraLobUnload.StreamColumnProcessors;
using Oracle.ManagedDataAccess.Client;

public class DataUnloader
{
    public int FileNameColumnIndex { get; init; } = 1;
    public int LobColumnIndex { get; init; } = 2;
    public string? OutputPath { get; init; } = null;
    public string? OutputFileExtension { get; init; } = null;

    public Action<string, string>? VisualFeedbackStartUnloading { get; init; }
    public Action? VisualFeedbackFinish { get; init; }

    private static readonly HashSet<string> _foldersCreated = new ();

    public void UnloadDataFromReader(OracleDataReader dataReader, IStreamColumnProcessor processor)
    {
        string cleanedFileNameExt = !string.IsNullOrEmpty(OutputFileExtension) ? "." + OutputFileExtension.Trim('.') : string.Empty;
        while (dataReader.Read())
        {
            string fileName = dataReader.GetString(FileNameColumnIndex - 1);
            fileName = Path.Combine(OutputPath ?? string.Empty, fileName);
            string fileNameWithExt = !string.IsNullOrEmpty(cleanedFileNameExt) && !fileName.EndsWith(cleanedFileNameExt, StringComparison.OrdinalIgnoreCase)
                ? fileName + cleanedFileNameExt
                : fileName;

            CreateFilePath(Path.GetDirectoryName(fileNameWithExt));
            using Stream outFile = new FileStream(fileNameWithExt, FileMode.Create, FileAccess.Write);
            using Stream lobContents = processor.ReadLob(dataReader, LobColumnIndex - 1);
            VisualFeedbackStartUnloading?.Invoke(fileNameWithExt, processor.GetFormattedLobLength(lobContents.Length));

            processor.SaveLobToStream(lobContents, outFile);
            VisualFeedbackFinish?.Invoke();

            lobContents.Close();
            outFile.Close();
        }
    }

    private static void CreateFilePath(string? filePath)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            if (!_foldersCreated.Contains(filePath))
            {
                Directory.CreateDirectory(filePath);
                _foldersCreated.Add(filePath);
            }
        }
    }
}
