namespace processJobAndSmsApi.Constants
{
    public static class ErrorMessages
    {
        public const string ACCESS_DENIED = "You cannot view page/link <strong class=\"badge bg-danger\">{0}</strong>";
        public const string GATEWAY_MISSING = "Gateway Missing. Please contact administrator.";
        public const string GATEWAY_INACTIVE = "Gateway Inactive. Please contact administrator.";
        public const string TIME_ERROR = "You can't send SMS as timing of selected route couldn't match.";
        public const string FILE_EXTENSION = "Please upload {0} extension file.";
        public const string FILE_SIZE = "Please upload file up to {0} MB max.";
        public const string EMPTY_FILE = "Please upload file with valid data.";
        public const string LOW_BALANCE = "You don't have enough SMS balance to send SMS messages.";
        public const string BLOCKED_NUMBERS = "There are errors in sending SMS as numbers might be invalid or blocked.";
        public const string ERROR_UPLOADING_FILE = "An error occurred during file upload. Please try to upload a valid CSV/XLS/TXT file with no formatting.";
        public const string OVERSELLING_LOW_BALANCE = "Error code: 200 occurred processing job. Please contact your account manager.";
        public const string OVER_LIMIT = "Error code: 201 occurred processing job. Please contact your account manager.";
        public const string SENDERID_BLOCKED = "Selected Sender ID has been blocked. Please contact your account manager.";
        public const string SENDERID_EMPTY = "Please select Sender ID.";
        public const string GROUP_EMPTY = "No numbers found in group(s).";
        public const string MESSAGE_EMPTY = "Please enter valid SMS message content.";
        public const string INVALID_NUMBERS = "Please enter valid 10-digit mobile numbers.";
        public const string SENDERID_INVALID = "Sender ID is invalid.";
        public const string SCHEDULE_ERROR = "Selected schedule date & time is invalid. Kindly keep at least {0} mins delay in scheduled date and time.";
        public const string SERVER_ERROR = "Something went wrong!";
        public const string DENIED_VSMS = "Please contact your account manager to access verified SMS.";
        public const string INVALID_VSMS_AGENT = "Incorrect/invalid verified SMS agent.";
        public const string REQUIRED_VSMS_AGENT = "Please select verified SMS agent.";
        public const string REQUIRED_NUMBERS = "Please provide phone numbers.";
        public const string JOB_ACCEPTED = "Warning - Campaign STARTED. If you refresh, campaign goes two times - no refund. You may check the status in reports after 10 min.";
    }
}
