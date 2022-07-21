namespace NoP77svk.OraLobUnload
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class SystemConsoleExt
    {
        public static string ReadLineInSecret(Func<char, char?> remapTheChar, bool cancelOnEscape = false)
        {
            return ReadLineInSecret((x) => remapTheChar(x)?.ToString(), cancelOnEscape);
        }

        public static string ReadLineInSecret(Func<char, string?> remapTheChar, bool cancelOnEscape = false)
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
                else if (cancelOnEscape && key.Key == ConsoleKey.Escape)
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
                        if (displayPartToRemove is not null and not "")
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

                    string? displayPartToAdd = remapTheChar(key.KeyChar);
                    if (displayPartToAdd is not null and not "")
                        Console.Error.Write(displayPartToAdd);
                    resultRemapped.Push(displayPartToAdd);
                }
            }

            return result.ToString();
        }
    }
}
