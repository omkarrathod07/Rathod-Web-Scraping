using System.Text.Json.Serialization;
namespace RathodWebScraping.Models
{
    public class ChatbotResponse
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public string system_fingerprint { get; set; }
        public Choice[] choices { get; set; }
        public string service_tier { get; set; }
        public Usage usage { get; set; }
    }
    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
        public Completion_Tokens_Details completion_tokens_details { get; set; }
    }
    public class Completion_Tokens_Details
    {
        public int reasoning_tokens { get; set; }
        public int accepted_prediction_tokens { get; set; }
        public int rejected_prediction_tokens { get; set; }
    }
    public class Choice
    {
        public int index { get; set; }
        public Message message { get; set; }
        public object logprobs { get; set; }
        public string finish_reason { get; set; }
    }
    public class Message
    {
        public string role { get; set; }
        public string content { get; set; }

        // used to instruct the JSON serializer to ignore the marked property
        // when serializing or deserializing JSON data.
        [JsonIgnore]
        public bool IsUser => role == "user";   
    }
}
