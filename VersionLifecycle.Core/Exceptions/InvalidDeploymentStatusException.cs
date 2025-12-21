namespace VersionLifecycle.Core.Exceptions;

/// <summary>
/// Exception thrown when an invalid deployment status transition is attempted.
/// </summary>
public class InvalidDeploymentStatusException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidDeploymentStatusException class.
    /// </summary>
    public InvalidDeploymentStatusException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance with message and inner exception.
    /// </summary>
    public InvalidDeploymentStatusException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
