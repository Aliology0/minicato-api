using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Application.Common.Models
{
    public class IncomingCallDto
    {
        public IncomingCallDto(string callSessionId, string callType, string channelName, string token, CallerInfo callerInfo)
        {
            CallSessionId = callSessionId;
            CallType = callType;
            ChannelName = channelName;
            Token = token;
            CallerInfo = callerInfo;
        }

        // Unique ID for the call session
        public string CallSessionId { get; set; } = string.Empty;

        // Type of call: "audio" or "video"
        public string CallType { get; set; } = "audio";

        // WebRTC or Agora channel name to join the call
        public string ChannelName { get; set; } = string.Empty;

        // Security token for the video/audio provider
        public string Token { get; set; } = null!;

        public CallerInfo CallerInfo { get; set; } = null!;
    }
    public record CallerInfo(int CallerId, string CallerName, string ProfileImage);
}
