namespace AlMostashar.Application.Common.Interfaces
{
    public interface IConnectionTracker
    {
        void AddConnection(int userID, string connectionId);

        void RemoveConnection(int userId, string connectionId);

        bool IsUserActive(int userId);
    }
}
