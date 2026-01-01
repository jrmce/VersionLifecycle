namespace VersionLifecycle.Core.Exceptions;

/// <summary>
/// Exception thrown when attempting to create a deployment that already exists.
/// </summary>
public class DuplicateDeploymentException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DuplicateDeploymentException class.
    /// </summary>
    public DuplicateDeploymentException(int applicationId, int versionId, int environmentId)
        : base($"A deployment for Application {applicationId}, Version {versionId} to Environment {environmentId} already exists.")
    {
        ApplicationId = applicationId;
        VersionId = versionId;
        EnvironmentId = environmentId;
    }

    /// <summary>
    /// Initializes a new instance with custom message.
    /// </summary>
    public DuplicateDeploymentException(string message) : base(message)
    {
    }

    /// <summary>
    /// The Application ID of the duplicate deployment.
    /// </summary>
    public int ApplicationId { get; }

    /// <summary>
    /// The Version ID of the duplicate deployment.
    /// </summary>
    public int VersionId { get; }

    /// <summary>
    /// The Environment ID of the duplicate deployment.
    /// </summary>
    public int EnvironmentId { get; }
}
