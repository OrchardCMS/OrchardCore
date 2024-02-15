using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OrchardCore.Common.Components;

public class ColloctedJSComponent : ComponentBase, IAsyncDisposable
{
    [Inject]
    private IJSRuntime? JS { get; set; }

    [Parameter]
    public bool RaiseDomEvents { get; set; }

    protected IJSObjectReference? JSModule { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var ns = GetType().Namespace;
            var cmp = GetType().Name;
            if (JS is not null)
            {
                JSModule = await JS.InvokeAsync<IJSObjectReference>("import",
                    $"./_content/{ns}/{cmp}.razor.js");
            }
        }
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (JSModule is not null)
        {
            await JSModule.DisposeAsync();
        }
    }

}
