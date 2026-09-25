namespace AlMostashar.Application.Common.Interfaces
{
    /// <summary>
    /// Marker interface for commands that should be wrapped in a database transaction.
    /// Apply this to any IRequest that modifies data and needs atomicity
    /// between SaveChanges and domain event handlers.
    /// </summary>
    public interface ITransactionalCommand
    {
    }
}
