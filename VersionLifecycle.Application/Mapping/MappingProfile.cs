namespace VersionLifecycle.Application.Mapping;

using AutoMapper;
using VersionLifecycle.Application.DTOs;
using VersionLifecycle.Application.Services;
using VersionLifecycle.Core.Entities;

/// <summary>
/// AutoMapper profile for all entity-to-DTO mappings.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Tenant mappings
        CreateMap<Tenant, TenantDto>();
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<Tenant, TenantLookupDto>();

        // Application mappings
        CreateMap<Application, ApplicationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId));
        CreateMap<CreateApplicationDto, Application>();
        CreateMap<UpdateApplicationDto, Application>();

        // Version mappings
        CreateMap<Version, VersionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId))
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Application != null ? src.Application.ExternalId : Guid.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateVersionDto, Version>();
        CreateMap<UpdateVersionDto, Version>();

        // Environment mappings
        CreateMap<Environment, EnvironmentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId));
        CreateMap<CreateEnvironmentDto, Environment>();
        CreateMap<UpdateEnvironmentDto, Environment>();

        // Deployment mappings
        CreateMap<Deployment, DeploymentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId))
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Application != null ? src.Application.ExternalId : Guid.Empty))
            .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.Version != null ? src.Version.ExternalId : Guid.Empty))
            .ForMember(dest => dest.EnvironmentId, opt => opt.MapFrom(src => src.Environment != null ? src.Environment.ExternalId : Guid.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApplicationName, opt => opt.MapFrom(src => src.Application != null ? src.Application.Name : null))
            .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src => src.Version != null ? src.Version.VersionNumber : null))
            .ForMember(dest => dest.EnvironmentName, opt => opt.MapFrom(src => src.Environment != null ? src.Environment.Name : null))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => src.ModifiedAt))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.CompletedAt));
        CreateMap<CreatePendingDeploymentDto, Deployment>();
        CreateMap<ConfirmDeploymentDto, Deployment>();

        // Deployment Event mappings
        CreateMap<DeploymentEvent, DeploymentEventDto>();

        // Webhook mappings
        CreateMap<Webhook, WebhookDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId))
            .ForMember(dest => dest.ApplicationId, opt => opt.MapFrom(src => src.Application != null ? src.Application.ExternalId : Guid.Empty));
        CreateMap<CreateWebhookDto, Webhook>();

        // Webhook Event mappings
        CreateMap<WebhookEvent, WebhookEventDto>()
            .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(src => src.DeliveryStatus));

        // API Token mappings
        CreateMap<ApiToken, ApiTokenDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ExternalId));
        CreateMap<CreateApiTokenDto, ApiToken>();
        CreateMap<UpdateApiTokenDto, ApiToken>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
