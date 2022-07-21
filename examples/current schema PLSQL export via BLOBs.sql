with
    function blob_listagg
        ( i_lines       in sys.ora_mining_varchar2_nt
        , i_prefix      in varchar2 default null
        , i_suffix      in varchar2 default null
        , i_charset     in varchar2 default 'al32utf8' )
        return blob
    is
        l_result        blob;
        i               pls_integer;
        
        procedure append_line
            ( i_line        in varchar2 )
        is
            l_chunk         raw(32767);
        begin
            l_chunk := utl_i18n.string_to_raw(i_line, i_charset);
            dbms_lob.writeAppend(l_result, utl_raw.length(l_chunk), l_chunk);
        end;
    begin
        if i_lines is not null or i_prefix is not null or i_suffix is not null then
            dbms_lob.createTemporary(l_result, true, dbms_lob.call);
        end if;
        
        if i_prefix is not null then
            append_line(i_prefix);
        end if;
        
        if i_lines is not null then
            i := i_lines.first();
            <<iterate_i_lines>>
            while i is not null loop
                append_line(i_lines(i));
                i := i_lines.next(i);
            end loop iterate_i_lines;
        end if;
        
        if i_suffix is not null then
            append_line(i_suffix);
        end if;
        
        return l_result;
    end;
    --
select
    -- put the exported file to the current folder...
    lower(S.type) -- ... and to the subfolder according to the object type...
        || '/' || lower(S.name) -- ... under the file name of the lower-case object name...
        || '.' ||
        decode(S.type,
            'PACKAGE', 'spc',
            'PACKAGE BODY', 'bdy',
            'TYPE', 'tps',
            'TYPE BODY', 'tpb',
            'FUNCTION', 'fnc',
            'PROCEDURE', 'prc',
            'TRIGGER', 'trg',
            'sql'
        ) -- ... and a file extension matching the object type
        as file_name_and_subpath,
    -- aggregate the file contents from the source lines
    blob_listagg(
        i_lines => cast(collect(S.text order by S.line) as sys.ora_mining_varchar2_nt),
        i_prefix => 'create or replace ',
        i_suffix => chr(10)||'/',
        i_charset => 'al32utf8'
    ) as file_contents
from user_source S
group by S.type, S.name
