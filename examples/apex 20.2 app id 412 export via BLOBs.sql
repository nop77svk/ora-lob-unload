with
    function apex_export$(i_app_id in integer)
        return apex_200200.wwv_flow_t_export_files
    is
    begin
        return apex_export.get_application(
            p_application_id => i_app_id,
            p_type => apex_export.C_TYPE_APPLICATION_SOURCE,
            p_split => true,
            p_with_date => true,
            p_with_ir_public_reports => true,
            p_with_ir_private_reports => false,
            p_with_ir_notifications => true,
            p_with_translations => true,
            p_with_pkg_app_mapping => true,
            p_with_original_ids => true,
            p_with_no_subscriptions => false,
            p_with_comments => true,
            p_with_supporting_objects => 'Y',
            p_with_acl_assignments => true,
            p_components => null
        );
    end;
    --
    function clob2blob(i_clob in clob, i_charset in varchar2)
        return blob
    is
        l_result                blob;
        l_csid                  integer := nls_charset_id('al32utf8');
        l_src_offset            integer := 1;
        l_dst_offset            integer := 1;
        l_ctx                   number := dbms_lob.DEFAULT_LANG_CTX;
        l_warning               number;
    begin
        dbms_lob.createTemporary(
            lob_loc => l_result,
            cache => true,
            dur => dbms_lob.CALL
        );
        dbms_lob.convertToBlob(
            dest_lob => l_result,
            src_clob => i_clob,
            amount => dbms_lob.LOBMAXSIZE,
            dest_offset => l_dst_offset,
            src_offset => l_src_offset,
            blob_csid => l_csid,
            lang_context => l_ctx,
            warning => l_warning
        );
        return l_result;
    end;
    --
select name, clob2blob(contents, 'al32utf8') as contents
from table(apex_export$(412))
