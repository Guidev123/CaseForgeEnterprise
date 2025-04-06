namespace CaseForge.Notifications;

public interface INotificator
{
    List<Notification> GetNotifications();

    void HandleNotification(Notification notification);

    bool HasNotification();
}