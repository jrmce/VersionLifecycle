namespace VersionLifecycle.Core.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the NotFoundException class.
    /// </summary>
    public NotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance with custom message.
    /// </summary>
    public NotFoundException(string message) : base(message)
    {
    }
}
