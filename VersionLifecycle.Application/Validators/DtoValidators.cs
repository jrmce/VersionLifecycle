namespace VersionLifecycle.Application.Validators;

using FluentValidation;
using VersionLifecycle.Application.DTOs;

/// <summary>
/// Validator for CreateApplicationDto.
/// </summary>
public class CreateApplicationValidator : AbstractValidator<CreateApplicationDto>
{
    public CreateApplicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Application name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.RepositoryUrl)
            .Must(uri => string.IsNullOrEmpty(uri) || Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Repository URL must be valid");
    }
}

/// <summary>
/// Validator for CreateVersionDto.
/// </summary>
public class CreateVersionValidator : AbstractValidator<CreateVersionDto>
{
    public CreateVersionValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0).WithMessage("Application ID is required");

        RuleFor(x => x.VersionNumber)
            .NotEmpty().WithMessage("Version number is required")
            .Matches(@"^\d+\.\d+(\.\d+)?$").WithMessage("Version number must follow semantic versioning (e.g., 1.0.0)");

        RuleFor(x => x.ReleaseNotes)
            .MaximumLength(5000).WithMessage("Release notes cannot exceed 5000 characters");
    }
}

/// <summary>
/// Validator for CreatePendingDeploymentDto.
/// </summary>
public class CreatePendingDeploymentValidator : AbstractValidator<CreatePendingDeploymentDto>
{
    public CreatePendingDeploymentValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0).WithMessage("Application ID is required");

        RuleFor(x => x.VersionId)
            .GreaterThan(0).WithMessage("Version ID is required");

        RuleFor(x => x.EnvironmentId)
            .GreaterThan(0).WithMessage("Environment ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}

/// <summary>
/// Validator for CreateEnvironmentDto.
/// </summary>
public class CreateEnvironmentValidator : AbstractValidator<CreateEnvironmentDto>
{
    public CreateEnvironmentValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0).WithMessage("Application ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Environment name is required")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be non-negative");
    }
}

/// <summary>
/// Validator for CreateWebhookDto.
/// </summary>
public class CreateWebhookValidator : AbstractValidator<CreateWebhookDto>
{
    public CreateWebhookValidator()
    {
        RuleFor(x => x.ApplicationId)
            .GreaterThan(0).WithMessage("Application ID is required");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Webhook URL is required")
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Webhook URL must be valid");

        RuleFor(x => x.Secret)
            .NotEmpty().WithMessage("Secret is required")
            .MinimumLength(16).WithMessage("Secret must be at least 16 characters");

        RuleFor(x => x.MaxRetries)
            .GreaterThan(0).WithMessage("Max retries must be greater than 0")
            .LessThanOrEqualTo(10).WithMessage("Max retries cannot exceed 10");
    }
}

/// <summary>
/// Validator for LoginDto.
/// </summary>
public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}

/// <summary>
/// Validator for RegisterDto.
/// </summary>
public class RegisterValidator : AbstractValidator<RegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain digit");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}
