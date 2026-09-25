using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Infrastructure.Services.AgoraEngine.TokenBuilders;
using Microsoft.Extensions.Configuration;

namespace AlMostashar.Infrastructure.Services
{
    public class AgoraService : ICommunicationService
    {
        private readonly IConfiguration _configuration;

        public AgoraService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateCallToken(string channelName, uint userId)
        {
            string appId = _configuration.GetValue<string>("Agora:AppId")!;
            string appCertificate = _configuration.GetValue<string>("Agora:Certificate")!;
            int TokenExpirationSeconds = _configuration.GetValue<int>("Agora:TokenExpirationSeconds");
            // 2. حساب وقت انتهاء صلاحية التوكن (الوقت الحالي + المدة بالثواني)
            uint currentTimestamp = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            uint privilegeExpiredTs = currentTimestamp + (uint)TokenExpirationSeconds;

            // 3. تحديد صلاحية المستخدم (يقدر يتكلم ويفتح كاميرا)
            RtcTokenBuilder2.Role role = RtcTokenBuilder2.Role.RolePublisher;

            // 4. استخدام الملف الرسمي لتوليد التوكن
            string token = RtcTokenBuilder2.buildTokenWithUid(
                appId,
                appCertificate,
                channelName,
                userId,
                role,
                privilegeExpiredTs, // وقت انتهاء التوكن نفسه
                privilegeExpiredTs  // وقت انتهاء صلاحية فتح المايك والكاميرا
            );

            return token;
        }
        public string GetAppId()
            => _configuration.GetValue<string>("Agora:AppId")!;


    }
}
