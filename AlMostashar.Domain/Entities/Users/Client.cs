namespace AlMostashar.Domain.Entities
{
    public class Client : User
    {
        public string? NationalIdPhotoUrl { get; set; }

        // Navigation — Step 4: 1:N (Client → ClientRequest via Create)
        public ICollection<ClientRequest> ClientRequests { get; set; } = new List<ClientRequest>();
    }
}
