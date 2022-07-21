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
select name, contents
from table(apex_export$(412))
