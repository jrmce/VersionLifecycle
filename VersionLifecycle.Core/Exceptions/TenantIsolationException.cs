namespace VersionLifecycle.Core.Exceptions;

/// <summary>
/// Exception thrown when tenant isolation is violated.
/// </summary>
public class TenantIsolationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the TenantIsolationException class.
    /// </summary>
    public TenantIsolationException() : base("Access denied: Tenant isolation violation.")
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message.
    /// </summary>
    public TenantIsolationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with a custom message and inner exception.
    /// </summary>
    public TenantIsolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
