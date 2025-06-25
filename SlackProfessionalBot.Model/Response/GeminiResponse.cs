using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackProfessionalBot.Model.Response
{
    // Helper classes to deserialize Gemini API response
    public class GeminiResponse
    {
        public Candidate[]? candidates { get; set; }
    }

    public class Candidate
    {
        public Content? content { get; set; }
        public string? finishReason { get; set; }
        public int? index { get; set; }
        public SafetyRating[]? safetyRatings { get; set; }
    }

    public class Content
    {
        public Part[]? parts { get; set; }
        public string? role { get; set; }
    }

    public class Part
    {
        public string? text { get; set; }
    }

    public class SafetyRating
    {
        public string? category { get; set; }
        public string? probability { get; set; }
    }
}
