        public class SendSmsRequest
        {
            public List<string> NumberArray { get; set; } = [];
            public Dictionary<string, object> AdditionalData { get; set; } = [];
            public Dictionary<string, object> Options2 { get; set; } = [];
            public object? MsgStat { get; set; }
            public bool SmsSend { get; set; } = false; // Default value
            public string? SmsParam { get; set; }
        }
