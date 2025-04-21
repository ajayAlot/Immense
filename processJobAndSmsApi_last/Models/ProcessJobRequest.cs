
using System.Text.Json.Serialization;


namespace processJobAndSmsApi.Models
{
    public class ProcessJobRequest
    {
        [JsonPropertyName("UserID")]
        public string? UserID { get; set; }
        
        [JsonPropertyName("LoggedUserId")]
        public string? LoggedUserId { get; set; }

        [JsonPropertyName("LoggedUserParentID")]
        public string? LoggedUserParentID { get; set; }
        
        [JsonPropertyName("job_id")]
        public string JobId { get; set; } = string.Empty;

        // [Required]
        // public string JobId { get; set; }


        [JsonPropertyName("job_status")]
        public string? JobStatus { get; set; }
        
        [JsonPropertyName("filepath")]
        public string? FilePath { get; set; }

        [JsonPropertyName("contact_filename")]
        public string? ContactFilename { get; set; }
        
        [JsonPropertyName("SMSCount")]
        public int? SMSCount { get; set; }

        [JsonPropertyName("messageID")]
        public string? messageID { get; set; }
        
        [JsonPropertyName("route_type_id")]
        public string? RouteTypeId { get; set; }
        
        [JsonPropertyName("sms_limit")]
        // [JsonConverter(typeof(StringToIntArrayConverter))]
        public int[] SmsLimit { get; set; } = [];
        
        [JsonPropertyName("TotalBalanceSMS")]
        public int TotalBalanceSMS { get; set; }
        
        [JsonPropertyName("TotalBalanceSMSAdminOverselling")]
        public int TotalBalanceSMSAdminOverselling { get; set; }
        
        [JsonPropertyName("OverSellingUsersCategoryName")]
        public string? OverSellingUsersCategoryName { get; set; }
        
        [JsonPropertyName("OverSellingActivated")]
        public string? OverSellingActivated { get; set; }
        
        [JsonPropertyName("OverSellingUsersID")]
        public string? OverSellingUsersID { get; set; }
        
        [JsonPropertyName("route")]
        public string? Route { get; set; }
        
        [JsonPropertyName("cutting")]
        public string? Cutting { get; set; }
        
        [JsonPropertyName("CutOffPercentage")]
        public int? CutOffPercentage { get; set; }
        
        [JsonPropertyName("CuttingMinValue")]
        public int? CuttingMinValue { get; set; }
        
        [JsonPropertyName("UserCategory")]
        public string? UserCategory { get; set; }
        
        [JsonPropertyName("UserFullName")]
        public string? UserFullName { get; set; }
        
        [JsonPropertyName("coding")]
        public int? Coding { get; set; }
        
        [JsonPropertyName("peid")]
        public string? Peid { get; set; }
        
        [JsonPropertyName("tid")]
        public string? Tid { get; set; }
        
        [JsonPropertyName("sender")]
        public string? Sender { get; set; }
        
        [JsonPropertyName("sender_name")]
        public string? SenderName { get; set; }
        
        [JsonPropertyName("Mclass")]
        public string? Mclass { get; set; }
        
        [JsonPropertyName("sms_message")]
        public string? SmsMessage { get; set; }
        
        [JsonPropertyName("CutoffStatusName")]
        public string? CutoffStatusName { get; set; }
        
        [JsonPropertyName("UserParentID")]
        public int? UserParentID { get; set; }
        
        [JsonPropertyName("ParentUserGroupID")]
        public int? ParentUserGroupID { get; set; }
        
        [JsonPropertyName("GatewayFamilyID")]
        public int? GatewayFamilyId { get; set; }
        
        [JsonPropertyName("MessageFormat")]
        public string? MessageFormat { get; set; }
        
        [JsonPropertyName("message_method")]
        public string? MessageMethod { get; set; }
        
        [JsonPropertyName("flash_sms")]
        public string? FlashSms { get; set; }
        
        [JsonPropertyName("schedule_sms")]
        public string? ScheduleSms { get; set; }
        
        [JsonPropertyName("schedule_pdate")]
        public string? ScheduleDate { get; set; }
        
        [JsonPropertyName("schedule_time")]
        public string? ScheduleTime { get; set; }
        
        [JsonPropertyName("message_type")]
        public string? MessageType { get; set; }
        
        [JsonPropertyName("sms_length")]
        public int? SmsLength { get; set; }

        [JsonPropertyName("smsMessageLength")]
        public int? SmsMessageLength { get; set; }

        
        [JsonPropertyName("urlencode_sms_message")]
        public string? UrlencodeSmsMessage { get; set; }
        
        [JsonPropertyName("SMPP_SMSC_ID")]
        public string? SMPPSMSCID { get; set; }
        
        [JsonPropertyName("SMPP_SMSC_ID_Name")]
        public string? SMPPSMSCIDName { get; set; }

        // ----------

        [JsonPropertyName("SmartSMSNo")]
        public int? SmartSMSNo { get; set; }

        [JsonPropertyName("DNDCount")]
        public int? DNDCount { get; set; }

        [JsonPropertyName("RefundCount")]
        public int? RefundCount { get; set; }

        [JsonPropertyName("BlockCount")]
        public int? BlockCount { get; set; }

        [JsonPropertyName("pdate")]
        public string? PDate { get; set; }

        [JsonPropertyName("jdate")]
        public string? JDate { get; set; }

        [JsonPropertyName("MessageStatus")]
        public string? MessageStatus { get; set; }


        // [JsonPropertyName("SMPP_SMSC_ID")]
        // public int[] SMPPSMSCID { get; set; } = Array.Empty<int>();
        // public List<int> SMPPSMSCID { get; set; } = new List<int>();

        // [JsonPropertyName("SMPP_SMSC_ID_Name")]
        // public string[]? SMPPSMSCIDName { get; set; } = Array.Empty<string>();
        //  public List<string> SMPPSMSCIDName { get; set; } = new List<string>();


        [JsonPropertyName("meta_data")]
        public string? MetaData { get; set; }
        
        [JsonPropertyName("charset")]
        public string? Charset { get; set; }
        
        [JsonPropertyName("sender_id")]
        public int? SenderId { get; set; }
        
        [JsonPropertyName("smart_filename")]
        public int? SmartFilename { get; set; }
        
        [JsonPropertyName("smsc")]
        public string? Smsc { get; set; }

        [JsonPropertyName("to")]
        public string? To { get; set; }
        [JsonPropertyName("add_to_group")]
        public string? AddToGroup { get; set; }
        [JsonPropertyName("group_id")]
        public string? GroupId { get; set; }
        [JsonPropertyName("send_notification")]
        public string? SendNotification { get; set; } 
        [JsonPropertyName("hits")]
        public string? Hits { get; set; } 

        [JsonPropertyName("cleanMessageSmartUrl")]
        public string? CleanMessageSmartUrl { get; set; }

        [JsonPropertyName("shortURL_Prefix")]
        public string? ShortURLPrefix { get; set; } = "imns.in/u/";
        
        [JsonPropertyName("hash_id")]
        public string? HashId { get; set; }
        
        [JsonPropertyName("RouteTypeData")]
        public RouteTypeData? RouteTypeData { get; set; }
                
        [JsonPropertyName("msg_stat")]
        public MsgStat? MsgStat { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }
        
        [JsonPropertyName("Access")]
        public Access? Access { get; set; }
        [JsonPropertyName("Options")]
        public Options? Options { get; set; }

    }


    public class MsgStat
    {
        [JsonPropertyName("pdu")]
        public int Pdu { get; set; }
        
        [JsonPropertyName("chars")]
        public int Chars { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        [JsonPropertyName("coding")]
        public string? Coding { get; set; }
        
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }
        
        [JsonPropertyName("max")]
        public int Max { get; set; }
    }

    public class RouteTypeData
    {
        [JsonPropertyName("balancer")]
        public string? Balancer { get; set; }
        
        [JsonPropertyName("country_id")]
        public int CountryId { get; set; }
        
        [JsonPropertyName("created_by_id")]
        public int CreatedById { get; set; }
        
        [JsonPropertyName("created_date")]
        public string? CreatedDate { get; set; }
        
        [JsonPropertyName("dnd_check")]
        public string? DNDCheck { get; set; }
        
        [JsonPropertyName("end_time")]
        public string? EndTime { get; set; }
        
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("ipaddress")]
        public string? IpAddress { get; set; }
        
        [JsonPropertyName("jdate")]
        public string? JDate { get; set; }
        
        [JsonPropertyName("modified_by_id")]
        public int ModifiedById { get; set; }
        
        [JsonPropertyName("modified_date")]
        public string? ModifiedDate { get; set; }
        
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        
        [JsonPropertyName("pdate")]
        public string? PDate { get; set; }
        
        [JsonPropertyName("ptime")]
        public string? PTime { get; set; }
        
        [JsonPropertyName("sender_id_status")]
        public string? SenderIdStatus { get; set; }
        
        [JsonPropertyName("start_time")]
        public string? StartTime { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
    public class Options
    {
        [JsonPropertyName("cutting_throughput")]
        public string CuttingThroughput { get; set; } = string.Empty;

        [JsonPropertyName("CutOffPercentage")]
        public int CutOffPercentage { get; set; }

        [JsonPropertyName("CuttingMinValue")]
        public int CuttingMinValue { get; set; }

        [JsonPropertyName("CutoffStatusName")]
        public string CutoffStatusName { get; set; } = string.Empty;

        [JsonPropertyName("cutting")]
        public string Cutting { get; set; } = "No";

        [JsonPropertyName("files")]
        public Files Files { get; set; } = new Files();

        [JsonPropertyName("data")]
        public Data Data { get; set; } = new Data();
    }

    public class Files
    {
        [JsonPropertyName("contact_file")]
        public ContactFile ContactFile { get; set; } = new ContactFile();
    }

    public class ContactFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("full_path")]
        public string FullPath { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("tmp_name")]
        public string TmpName { get; set; } = string.Empty;

        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("message_type")]
        public string? MessageType { get; set; }

        [JsonPropertyName("message_smart_url")]
        public string? MessageSmartUrl { get; set; }

        [JsonPropertyName("tempid")]
        public string Tempid { get; set; } = string.Empty;

        [JsonPropertyName("route_type_id")]
        public string RouteTypeId { get; set; } = string.Empty;

        [JsonPropertyName("sender_id")]
        public string SenderId { get; set; } = string.Empty;

        [JsonPropertyName("message_format")]
        public string? MessageFormat { get; set; }

        [JsonPropertyName("checkAll")]
        public string CheckAll { get; set; } = "0";

        [JsonPropertyName("users_group_id")]
        public int[] UsersGroupId { get; set; } = [];

        [JsonPropertyName("my_draft")]
        public string MyDraft { get; set; } = string.Empty;

        [JsonPropertyName("sms_message")]
        public string SmsMessage { get; set; } = string.Empty;

        [JsonPropertyName("templateid")]
        public string TemplateId { get; set; } = string.Empty;

        [JsonPropertyName("schedule_date")]
        public string ScheduleDate { get; set; } = string.Empty;

        [JsonPropertyName("schedule_time")]
        public string ScheduleTime { get; set; } = string.Empty;

        [JsonPropertyName("cmd")]
        public string Cmd { get; set; } = "send-sms";

        [JsonPropertyName("NumberCount")]
        public int NumberCount { get; set; }

        [JsonPropertyName("UniqueNumberCount")]
        public int UniqueNumberCount { get; set; }

        [JsonPropertyName("TotalNumberCount")]
        public int TotalNumberCount { get; set; }
    }
    public class Access
    {
        [JsonPropertyName("allow_smpp")]
        public string AllowSmpp { get; set; } = string.Empty;

        [JsonPropertyName("allow_spam")]
        public string AllowSpam { get; set; } = string.Empty;

        [JsonPropertyName("api_access")]
        public string ApiAccess { get; set; } = string.Empty;

        [JsonPropertyName("apply_tree")]
        public string ApplyTree { get; set; } = string.Empty;

        [JsonPropertyName("apply_tree_cutting")]
        public string? ApplyTreeCutting { get; set; } // Nullable

        [JsonPropertyName("campaign")]
        public string Campaign { get; set; } = string.Empty;

        [JsonPropertyName("compose")]
        public string Compose { get; set; } = string.Empty;

        [JsonPropertyName("dnd_refund")]
        public string DndRefund { get; set; } = string.Empty;

        [JsonPropertyName("dynamic_sender")]
        public string DynamicSender { get; set; } = string.Empty;

        [JsonPropertyName("dynamic_sms")]
        public string DynamicSms { get; set; } = string.Empty;

        [JsonPropertyName("flash_sms")]
        public string FlashSms { get; set; } = string.Empty;

        [JsonPropertyName("gateway_family_id")]
        public string GatewayFamilyId { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("number_management")]
        public string NumberManagement { get; set; } = string.Empty;

        [JsonPropertyName("on_refund")]
        public string OnRefund { get; set; } = string.Empty;

        [JsonPropertyName("overselling")]
        public string Overselling { get; set; } = string.Empty;

        [JsonPropertyName("rcs_msg")]
        public string RcsMsg { get; set; } = string.Empty;

        [JsonPropertyName("region_id")]
        public string RegionId { get; set; } = string.Empty;

        [JsonPropertyName("report_block")]
        public string ReportBlock { get; set; } = string.Empty;

        [JsonPropertyName("restricted_report")]
        public string RestrictedReport { get; set; } = string.Empty;

        [JsonPropertyName("restricted_template")]
        public string RestrictedTemplate { get; set; } = string.Empty;

        [JsonPropertyName("smart_sms")]
        public string SmartSms { get; set; } = string.Empty;

        [JsonPropertyName("sms_limit")]
        public int SmsLimit { get; set; } // This is now an integer

        [JsonPropertyName("template")]
        public string Template { get; set; } = string.Empty;

        [JsonPropertyName("text_sms")]
        public string TextSms { get; set; } = string.Empty;

        [JsonPropertyName("unicode_sms")]
        public string UnicodeSms { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("verified_sms")]
        public string VerifiedSms { get; set; } = string.Empty;
    }


}