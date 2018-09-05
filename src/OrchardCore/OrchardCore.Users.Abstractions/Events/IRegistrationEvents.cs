using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Users.Events
{
    public interface IRegistrationEvents
    {
        Task RegisteringAsync(Action<string, string> reportError);

        Task RegisteredAsync();
    }
}