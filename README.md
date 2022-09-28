# Oracle LOB unloader

<a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/"><img alt="Creative Commons License" style="border-width:0" src="https://i.creativecommons.org/l/by-sa/4.0/88x31.png" /></a><br />Licensed under a <a rel="license" href="http://creativecommons.org/licenses/by-sa/4.0/">Creative Commons Attribution-ShareAlike 4.0 International License</a>.

## The problem...
Ever needed to get those damned CLOBs/BLOBs out of the database and save them to your local PC's drive?

Look no further, for this utility is what you'll ever need! 😎

## The solution...

Download your Oracle LOB Unloader build from the [Releases](https://github.com/nop77svk/ora-lob-unload/releases) page, unzip, run as `dotnet ora_lob_unload.dll --help` and see what happens. (You're going to need the [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) for the *Portable* build to work.)

Oh, you don't yet see the possibilities?

### _Example_: Exporting APEX application from database to disk (for source control)

Save [the export query](https://github.com/nop77svk/ora-lob-unload/blob/main/examples/apex%2020.2%20app%20id%20412%20export.sql) into a file (e.g., `apex_app_412_export.sql`) on your local drive. Run the LOB unloader as

```
dotnet ora_lob_unload.dll -u myuser/mypassword@mydatabase -q -i apex_app_412_export.sql -o ./apex_app_412_export -x sql
```

so that it connects (`-u`) to the `mydatabase` DB as the `myuser` user with the `mypassword` password, use the query (`-q`) from the `apex_app_412_export.sql` script file as input (`-i`) and the current folder's subfolder `apex_app_412_export` for placing (`-o`) the unloaded files into. Convert the CLOBs being read to the `utf-8` charset (the default of the `--clob-output-charset` parameter) and append the `.sql` extension (`-x`) to each of the unloaded files if not already appended.

The query produces at least 2 columns with the file name being in the first (the default of the `--file-name-column-ix` parameter) column and file contents being the in the second (the default of the `--lob-column-ix` parameter) column of the query. The query itself may project arbitrary number of columns for that matter.

### _Example_: Exporting PLSQL sources from the current schema

Save [the export query](https://github.com/nop77svk/ora-lob-unload/blob/main/examples/current%20schema%20PLSQL%20export%20via%20BLOBs.sql) into a file (e.g., `schema_plsql_export.sql`) on your local drive and run the LOB unloader as

```
dotnet ora_lob_unload.dll -u myuser/mypassword@mydatabase -q -i schema_plsql_export.sql -o ./myuser_schema_export
```

Principle is the same as for the prior example.

## Technical issues

There's a bug in `Oracle.ManagedDataAccess.Core`'s handling of `Oracle.Types.OracleClob` streamed reading which actually renders this whole utility useless for CLOBs. Although I reported the bug to the Oracle .NET team and they managed to successfully reproduce it (and filed under id 32671328 on March 24, 2021), until it's resolved, there's not much sense in using this app for offloading CLOBs. However...!

You can offload BLOBs and BFILEs without restriction. If you are skilled enough in PL/SQL, you can convert your CLOBs to binary data on your database side and offload them this way. You are restricted only by your imagination and creativity. 😉 However, feel free to get inspired by [the examples in the repository](https://github.com/nop77svk/ora-lob-unload/tree/main/examples).

### Update, 2022/09/28

Oracle's .NET team e-mailed me about the bug having been fixed. Testing of the fix is currently ongoing.

## A word from+about the author

As a long-time (15+ years) Oracle PL/SQL dev who's hit the boundaries of PL/SQL's capabilities (even with pretty heavy use of Oracle's OOP), I wanted to expand on the options of expressing myself via computer code. And while Java still seems to be widely more popular across the world, I've chosen C# and .NET (Core) as the "middle tier" platform of my choice simply because I somehow (irrationally, perhaps) liked the C# language more than the Java language. C# seems more concise, more readable, more pragmatic than Java and ever since I found out about the wonderful cross-platform .NET Core, my "destiny" seemed clear.

So, here it is! A simple "show-off" project of mine I wrote during learning the C#, .NET Core, different OOP techniques (that were not possible anymore within my beloved PL/SQL's "mantinels") and various code writing principles of C# world. It's the very first, so it's not the nicest. Everyone learns, you know. 😇
