using System.Collections.Specialized;

namespace processJobAndSmsApi.Models
{

    public class DlrRequest
    {
        public DateTime DateTimeReceived { get; set; } = DateTime.MinValue;
        public string HexUniqueId { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public Dictionary<string, string> QueryString { get; set; } // ✅ Change to Dictionary
    }

    public class DLRData
    {
        // public Guid Id { get; set; } = Guid.Empty;
        public string Sub { get; set; } = string.Empty;
        public string Dlvrd { get; set; } = string.Empty;
        public DateTime SubmitDate { get; set; } = DateTime.MinValue;
        public DateTime DoneDate { get; set; } = DateTime.MinValue;
        public string Stat { get; set; } = string.Empty;
        public string Err { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

}
