namespace AlMostashar.Application.Common.Interfaces
{
    public interface ICommunicationService
    {
        string GenerateCallToken(string channelName, uint userId);
        string GetAppId();
    }
}
