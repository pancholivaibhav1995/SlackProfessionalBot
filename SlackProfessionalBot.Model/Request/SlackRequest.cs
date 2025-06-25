using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackProfessionalBot.Model.Request
{
    public class SlackRequest
    {
        public string Token { get; set; }
        public string TeamDomain { get; set; }
        public string UserId { get; set; }
        public string Command { get; set; }
        public string Text { get; set; }
        public string ResponseUrl { get; set; }
    }
}
