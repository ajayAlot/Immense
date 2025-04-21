namespace processJobAndSmsApi.Constants
{
    public static class SmsConfig
{
    public const string SMS_CGI = "http://" + Localhost + ":15013/cgi-bin/sendsms";
    public static readonly int[] SMS_PORTS = [15013];
    public const string DLR_URL = "http://" + Localhost + ":5195/api/UpdateDLR/process-dlr?to=%P&from=%p&ack=%a&dlr-mask=%d&status=%d&smsc=%i&job_id=${job_id}&msg_id=${msg_id}&user_id=${user_id}";
    // Define constants
    public const string TLV_TAG_PEID = "EntityID=";
    public const string TLV_TAG_TID = "ContentID=";
    public const string TLV_TAG_HASH = "HASH=";
    public const string TLV_TAG_TMID = "HASH="; 
    public const string TLV_METADATA = TLV_TAG_PEID + "{0}&" + TLV_TAG_TID + "{1}&" + TLV_TAG_HASH + "{2}&"; 

    public const string TLV_SMPP = "?smpp?";
    // public const string TLV_INFO = TLV_METADATA + "job_id=%s&msg_id=${{msg_id}}&user_id=%s";
    // public const string TLV_INFO = "EntityID={0}&ContentID={1}&job_id={2}&msg_id=${msg_id}&user_id={3}";
    // public const string TLV_INFO = "EntityID={0}&ContentID={1}&HASH={2}&job_id={3}&msg_id=${{msg_id}}{4}&user_id={5}";
    public const string TLV_INFO = TLV_METADATA + "job_id={3}&msg_id=${{msg_id}}&user_id={4}";
    
    public const string JOB_PH = "${job_id}";
    public const string USER_PH = "${user_id}";
    public const string MSG_PH = "${msg_id}";
    public const string SMS_USER = "testuser";
    public const string SMS_PSWD = "testuser";
    public const int DLR_MASK = 19;
    public const string SMS_BGS = "cron/bg-sms.php";
 
    private const string Localhost = "127.0.0.1";  
    
    public static readonly List<string> DND_ERROR_CODE_LIST = new List<string> { "640", "650", "413", "74", "074" };

    public const int DLR_BATCH_SIZE = 1000; 
    public const int DLR_LARGE_BATCH_SIZE = 100000; 
}




}