using System;

namespace OraLobUnload
{
    static class InputSqlReturnTypeEnumHelpers
    {
        static internal InputSqlReturnTypeEnum GetUltimateScriptType(CommandLineOptions options)
        {
            InputSqlReturnTypeEnum result = options.InputSqlReturnTypeSelect ?
                InputSqlReturnTypeEnum.Select :
                options.InputSqlReturnTypeScalars ?
                    InputSqlReturnTypeEnum.Scalars :
                    options.InputSqlReturnTypeCursor ?
                        InputSqlReturnTypeEnum.RefCursor :
                        options.InputSqlReturnTypeMultiImplicit ?
                            InputSqlReturnTypeEnum.MultiImplicitCursors :
                            options.InputSqlReturnTypeTable ?
                                InputSqlReturnTypeEnum.Table :
                                options.InputSqlReturnTypeStr.ToLower() switch
                                {
                                    "select" or "query" or "" => InputSqlReturnTypeEnum.Select,
                                    "cursor" or "refcursor" or "ref-cursor" => InputSqlReturnTypeEnum.RefCursor,
                                    "multi-implicit" or "implicit" or "implicit-cursor" or "implicit-cursors" => InputSqlReturnTypeEnum.MultiImplicitCursors,
                                    "scalar" or "scalars" => InputSqlReturnTypeEnum.Scalars,
                                    "table" => InputSqlReturnTypeEnum.Table,
                                    _ => throw new ArgumentOutOfRangeException($"Don't know how to handle input script type \"{options.InputSqlReturnTypeStr}\"")
                                };
            return result;
        }
    }
}
