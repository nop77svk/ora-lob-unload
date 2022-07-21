# Oracle LOB unloader

Ever needed to get those damned CLOBs/BLOBs out of the database and save them to your local PC's drive?

Look no further, for this utility is what you'll ever need. :-)

Not yet?
========

There's a bug in handling of Oracle.ManagedDataAccess.Core's Oracle.Types.OracleClob streamed reading which actually renders this whole utility useless. Although I reported the bug to the Oracle .NET team and they managed to successfully reproduce it (and filed under id 32671328 on March 24, 2021), until it's resolved, there's not much sense in using this app for offloading CLOBs. However...!

You can offload BLOBs and BFILEs without restriction. If you are skilled enough in PL/SQL, you can convert your CLOBs to binary data on your database side and offload them this way.

How...?
=======

<2do!>

A note from the author
======================

As a long-time (15+ years) Oracle PL/SQL dev who's hit the boundaries of PL/SQL's capabilities (even with pretty heavy use of Oracle's OOP), I wanted to expand on the options of expressing myself via computer code. And while Java still seems to be widely more popular across the world, I've chosen C# and .NET (Core) as the "middle tier" platform of my choice simply because I somehow (irrationally, perhaps) liked the C# language more than the Java language. C# seems more concise, more readable, more pragmatic than Java and ever since I found out about the wonderful cross-platform .NET Core, my "destiny" seemed clear.

So, here it is! A simple "show-off" project of mine I wrote during learning the C#, .NET Core, different OOP techniques (that were not possible anymore within my beloved PL/SQL's "mantinels") and various code writing principles of C# world.
