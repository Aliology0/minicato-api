namespace AlMostashar.Infrastructure.Services.AgoraEngine.Common
{
    public interface IPackable
    {
        ByteBuf marshal(ByteBuf outBuf);
    }


}
