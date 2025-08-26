using DetailViewer.Core.Models;
using System;

namespace DetailViewer.Core.Interfaces
{
    public interface IActiveUserService
    {
        Profile CurrentUser { get; set; }
        event Action CurrentUserChanged;
    }
}
