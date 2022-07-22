namespace NoP77svk.OraLobUnload.Utilities;

using System;
using System.Collections.Generic;
using System.Text;

public class SecretSystemConsole
{
    public bool CancelOnEscape { get; init; } = false;

    public Func<char, string> ObfuscateTheInputChar { get; init; }

    public SecretSystemConsole(Func<char, string> obfuscateTheInputChar)
    {
        ObfuscateTheInputChar = obfuscateTheInputChar;
    }

    public SecretSystemConsole(Func<char, char> obfuscateTheInputChar)
    {
        ObfuscateTheInputChar = x => obfuscateTheInputChar(x).ToString();
    }

    public string ReadLineInSecret()
    {
        if (Console.IsInputRedirected)
            throw new NotImplementedException("Framework restriction: Cannot secretly input password when standard input redirection is in effect");

        StringBuilder result = new StringBuilder();
        Stack<string?> resultRemapped = new Stack<string?>();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.Error.WriteLine();
                break;
            }
            else if (CancelOnEscape && key.Key == ConsoleKey.Escape)
            {
                result.Clear();
                Console.Error.WriteLine("<esc!>");
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (result.Length > 0)
                {
                    result.Remove(result.Length - 1, 1);

                    string? displayPartToRemove = resultRemapped.Pop();
                    if (!string.IsNullOrEmpty(displayPartToRemove))
                    {
                        Console.CursorLeft -= displayPartToRemove.Length;
                        Console.Error.Write(new string(' ', displayPartToRemove.Length));
                        Console.CursorLeft -= displayPartToRemove.Length;
                    }
                }
            }
            else if (key.KeyChar != '\0')
            {
                result.Append(key.KeyChar);

                string? displayPartToAdd = ObfuscateTheInputChar(key.KeyChar);
                if (!string.IsNullOrEmpty(displayPartToAdd))
                    Console.Error.Write(displayPartToAdd);
                resultRemapped.Push(displayPartToAdd);
            }
        }

        return result.ToString();
    }
}
