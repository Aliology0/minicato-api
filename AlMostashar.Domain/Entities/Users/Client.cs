namespace AlMostashar.Domain.Entities
{
    public class Client : User
    {
        // Navigation — Step 4: 1:N (Client → ClientRequest via Create)
        public ICollection<ClientRequest> ClientRequests { get; set; } = new List<ClientRequest>();

        // Location fields
        public int GovernorateId { get; set; }
        public string? Governorate { get; set; }
        public int CityId { get; set; }
        public string? City { get; set; }
    }
}
