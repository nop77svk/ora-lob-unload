with
    function blob_listagg
        ( i_lines                       in sys.ora_mining_varchar2_nt
        , i_prefix                      in varchar2 default null
        , i_suffix                      in varchar2 default null
        , i_charset                     in varchar2 default 'al32utf8' )
        return blob
    is
        l_result                        blob;
        i                               pls_integer;
            
        procedure append_chunk
            ( i_chunk                       in varchar2 )
        is
            l_raw_chunk                     raw(32767);
        begin
            l_raw_chunk := utl_i18n.string_to_raw(i_chunk, i_charset);
            dbms_lob.writeAppend(l_result, utl_raw.length(l_raw_chunk), l_raw_chunk);
        end;
    begin
        if i_lines is not null or i_prefix is not null or i_suffix is not null then
            dbms_lob.createTemporary(l_result, true, dbms_lob.call);
        end if;
        
        if i_prefix is not null then
            append_chunk(i_prefix);
        end if;
        
        if i_lines is not null then
            i := i_lines.first();
            <<iterate_i_lines>>
            while i is not null loop
                append_chunk(i_lines(i));
                i := i_lines.next(i);
            end loop iterate_i_lines;
        end if;
        
        if i_suffix is not null then
            append_chunk(i_suffix);
        end if;
        
        return l_result;
    end;
    --
    function clob2blob
        ( i_value                       in clob
        , i_charset                     in varchar2 default 'al32utf8' )
        return blob
    is
        l_result                        blob;
        l_dest_offset                   integer := 1;
        l_src_offset                    integer := 1;
        l_lang_ctx                      number := dbms_lob.DEFAULT_LANG_CTX;
        l_warning                       number;
    begin
        dbms_lob.createTemporary(l_result, true, dbms_lob.CALL);
        dbms_lob.convertToBlob(
            dest_lob => l_result,
            src_clob => i_value,
            amount => dbms_lob.LOBMAXSIZE,
            dest_offset => l_dest_offset,
            src_offset => l_src_offset,
            blob_csid => nls_charset_id(i_charset),
            lang_context => l_lang_ctx,
            warning => l_warning
        );
        return l_result;
    end;
--
select
    -- put the exported file to the current folder...
    lower(X.object_type) -- ... and to the subfolder according to the object type...
        || '/' || lower(X.object_name) -- ... under the file name of the lower-case object name...
        || '.' ||
        decode(X.object_type,
            'PACKAGE', 'spc',
            'PACKAGE BODY', 'bdy',
            'TYPE', 'tps',
            'TYPE BODY', 'tpb',
            'FUNCTION', 'fnc',
            'PROCEDURE', 'prc',
            'TRIGGER', 'trg',
            'VIEW', 'vw',
            'sql'
        )
        as file_name,
    X.object_ddl as file_contents
from (
        select --+ no_merge noparallel
            sys_context('userenv', 'session_user') as owner, S.type as object_type, S.name as object_name,
            blob_listagg(
                i_lines => cast(collect(S.text order by S.line asc) as sys.ora_mining_varchar2_nt),
                i_prefix => 'create or replace ',
                i_suffix => chr(10)||'/'||chr(10)
            ) as object_ddl
        from user_source S
        group by S.type, S.name
        --
        union all
        --
        select --+ leading(U) no_merge(U)
            sys_context('userenv', 'session_user') as owner, 'VIEW', V.view_name,
            clob2blob(to_clob('create or replace view '
                || case when regexp_like(V.view_name, '^[A-Z][A-Z0-9$#_]*$') then lower(V.view_name) else '"'||V.view_name||'"' end
                || chr(10) || '    bequeath '||lower(V.bequeath)
                || chr(10) || 'as'
                || chr(10) )
                || V.text
            ) as source
        from xmltable('/ROWSET/ROW'
                passing dbms_xmlgen.getXmlType('
                    select view_name, bequeath, text
                    from user_views
                ')
                columns
                    view_name       varchar2(128),
                    bequeath        varchar2(12),
                    text            clob
            ) V
    ) X

