using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.ContentLocalization
{
    public interface ILocalizable
    {
        string LocalizationSet { get; }
        string Culture { get; }
    }
}
