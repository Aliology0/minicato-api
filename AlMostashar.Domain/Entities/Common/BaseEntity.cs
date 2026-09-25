using AlMostashar.Domain.Entities.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlMostashar.Domain.Entities
{
    public class BaseEntity
    {
        public int Id { get; set; }

        [NotMapped]
        public List<BaseEvent> DomainEvents { get; private set; } = new();

        protected void AddEvent(BaseEvent @event)
        {
            DomainEvents.Add(@event);
        }

        public void ClearDomainEvents()
        {
            DomainEvents.Clear();
        }
    }
}
