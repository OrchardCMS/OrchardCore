using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Users.Events
{
    public interface IRegistrationEvents
    {
        Task RegisteringAsync(Action<string, string> reportError);

        Task RegisteredAsync();
    }
}