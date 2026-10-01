namespace AlMostashar.Domain.Entities
{
    public class Client : User
    {
        // Navigation — Step 4: 1:N (Client → ClientRequest via Create)
        public ICollection<ClientRequest> ClientRequests { get; set; } = new List<ClientRequest>();
    }
}
