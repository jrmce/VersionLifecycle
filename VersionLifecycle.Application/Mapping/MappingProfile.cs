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
        CreateMap<Application, ApplicationDto>();
        CreateMap<CreateApplicationDto, Application>();
        CreateMap<UpdateApplicationDto, Application>();

        // Version mappings
        CreateMap<Version, VersionDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateVersionDto, Version>();
        CreateMap<UpdateVersionDto, Version>();

        // Environment mappings
        CreateMap<Environment, EnvironmentDto>();
        CreateMap<CreateEnvironmentDto, Environment>();
        CreateMap<UpdateEnvironmentDto, Environment>();

        // Deployment mappings
        CreateMap<Deployment, DeploymentDto>()
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
        CreateMap<Webhook, WebhookDto>();
        CreateMap<CreateWebhookDto, Webhook>();

        // Webhook Event mappings
        CreateMap<WebhookEvent, WebhookEventDto>()
            .ForMember(dest => dest.DeliveryStatus, opt => opt.MapFrom(src => src.DeliveryStatus));

        // API Token mappings
        CreateMap<ApiToken, ApiTokenDto>();
        CreateMap<CreateApiTokenDto, ApiToken>();
        CreateMap<UpdateApiTokenDto, ApiToken>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
