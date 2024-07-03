using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Modules;

namespace BaseThemeSample;

public class BaseThemeFeatureIndependentStartup : StartupBase 
{ 
}

[Feature("BaseThemeSample")]
public class BaseThemeSampleStartup : StartupBase 
{
}
