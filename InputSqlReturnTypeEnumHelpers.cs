namespace OraLobUnload
{
    using System;

    internal static class InputSqlReturnTypeEnumHelpers
    {
        internal static InputSqlReturnTypeEnum GetUltimateScriptType(CommandLineOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options), "NULL supplied; this smells of fatal error!");

            InputSqlReturnTypeEnum result;
            if (options.InputSqlReturnTypeTable)
                result = InputSqlReturnTypeEnum.Table;
            else if (options.InputSqlReturnTypeSelect)
                result = InputSqlReturnTypeEnum.Select;
            else if (options.InputSqlReturnTypeScalars)
                result = InputSqlReturnTypeEnum.Scalars;
            else if (options.InputSqlReturnTypeCursor)
                result = InputSqlReturnTypeEnum.RefCursor;
            else if (options.InputSqlReturnTypeMultiImplicit)
                result = InputSqlReturnTypeEnum.MultiImplicitCursors;
            else
                throw new ArgumentOutOfRangeException(nameof(options), "No input SQL return type specified");

            return result;
        }
    }
}
