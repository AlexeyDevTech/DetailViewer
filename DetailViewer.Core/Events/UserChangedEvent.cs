using Prism.Events;

namespace DetailViewer.Core.Events
{
    /// <summary>
    /// Событие, сигнализирующее о смене текущего пользователя в системе.
    /// Передает объект <see cref="Models.Profile"/> нового пользователя.
    /// </summary>
    public class UserChangedEvent : PubSubEvent<Models.Profile> { }
}