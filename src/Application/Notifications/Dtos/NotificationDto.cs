using RMS.Domain.Entities;

namespace RMS.Application.Notifications.Dtos;

public record NotificationDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required NotificationType Type { get; init; }
    public Guid? RelatedApplicationId { get; init; }
    public required bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public required DateTimeOffset Created { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<NotificationRecipient, NotificationDto>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.NotificationId))
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Notification.Title))
                .ForMember(d => d.Body, o => o.MapFrom(s => s.Notification.Body))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Notification.Type))
                .ForMember(d => d.RelatedApplicationId, o => o.MapFrom(s => s.Notification.RelatedApplicationId))
                .ForMember(d => d.IsRead, o => o.MapFrom(s => s.IsRead))
                .ForMember(d => d.ReadAt, o => o.MapFrom(s => s.ReadAt))
                .ForMember(d => d.Created, o => o.MapFrom(s => s.Notification.Created));
        }
    }
}
