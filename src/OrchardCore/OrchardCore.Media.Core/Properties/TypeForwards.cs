using System.Runtime.CompilerServices;
using OrchardCore.Media.Core.Processing;

// These image-processing enums moved to OrchardCore.Media.Abstractions so the public image
// processing engine contract can reference them without creating an assembly reference cycle.
// The namespace is unchanged, so source consumers are unaffected; these forwarders preserve
// binary compatibility for code compiled against OrchardCore.Media.Core.
[assembly: TypeForwardedTo(typeof(ResizeMode))]
[assembly: TypeForwardedTo(typeof(Format))]
