using Prism.Events;

namespace DetailViewer.Core.Events
{
    /// <summary>
    /// Событие для обновления статуса приложения, передающее строковое сообщение.
    /// </summary>
    public class StatusUpdateEvent : PubSubEvent<string>
    {
    }
}