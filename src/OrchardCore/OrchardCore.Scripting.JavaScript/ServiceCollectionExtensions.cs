using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Jint;
using Jint.Constraints;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Scripting.JavaScript;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJavaScriptEngine(this IServiceCollection services)
    {
        services.AddSingleton<IScriptingEngine, JavaScriptEngine>();

        services.Configure<Options>(option =>
        {
            option.ExperimentalFeatures |= ExperimentalFeature.TaskInterop;

            // The cancellation token IScriptingManager.EvaluateAsync() accepts has to reach the interpreter
            // somehow, and the built-in helpers cannot carry it: Options.CancellationToken() takes the token
            // when the options are configured, and one Jint.Options instance serves every engine of the
            // tenant for the lifetime of the tenant, while the token belongs to one evaluation.
            //
            // OperationDeadlineConstraint is the shape that fits. The host arms it around the work it wants
            // bounded and disarms it afterwards, so JavaScriptEngine.EvaluateAsync() can hand it the token
            // of the call it is serving. A factory rather than an instance, because the options are shared:
            // each engine has to get its own, or two concurrent evaluations would arm and disarm the same
            // one. Disarmed - which is every synchronous evaluation, and every asynchronous one whose token
            // cannot be cancelled - a check reads two fields and takes no timestamp, and the constraint
            // declares itself amortizable, so the interpreter's tight-loop lane stays armed either way.
            option.Constraint(static () => new OperationDeadlineConstraint());

            // Scripts here are authored through the admin UI rather than shipped with the application, so
            // the engine runs code this project did not write. Without this probe an unbounded recursion
            // does not raise an exception at all: the native stack overflows, the runtime kills the
            // process, and no 'catch', no logger and no error page sees it. With it the same script raises
            // an ordinary JavaScript error, and the engine is still usable afterwards.
            //
            // The execution constraints a site can configure do not cover this. A budget of statements or
            // of wall-clock time is only reached if the script survives long enough to spend it, and an
            // overflow arrives in milliseconds - both of the settings this module's documentation offers as
            // examples, MaxStatements(10_000) and TimeoutInterval(5s), still end in a dead process.
            //
            // It is set here, on the registration, rather than on the engine, so that an application that
            // has measured the cost on its own scripts and does not want it can turn it off with its own
            // Configure<Jint.Options> call.
            option.Constraints.StackOverflowGuard = true;

            // The dynamic wrappers are unwrapped to the node they carry, because a DynamicObject exposes
            // nothing a script can read. The type Jint hands over describes the member the value was read
            // from, not the node it is replaced by, and the two are unrelated: JsonDynamicObject derives
            // from DynamicObject, not from JsonObject. A type of JsonDynamicObject would therefore have
            // members resolved against one type and invoked on another. Nothing declares a member as a
            // JsonDynamic type today - they surface as dynamic, which Jint maps back to the runtime type -
            // so passing no type at all is what keeps that unreachable rather than merely unlikely: the
            // wrapper then describes the node it actually holds. StringValues is left as it is, because
            // there the exposed type is one a member really can declare, so dropping it would change what
            // a script sees rather than only close a hole.
            option.SetWrapObjectHandler(static (e, target, type) => target switch
            {
                JsonDynamicObject dynamicObject => ObjectWrapper.Create(e, (JsonObject)dynamicObject),
                JsonDynamicArray dynamicArray => ObjectWrapper.Create(e, (JsonArray)dynamicArray),
                JsonDynamicValue dynamicValue => ObjectWrapper.Create(e, (JsonValue)dynamicValue),
                StringValues stringValues => ObjectWrapper.Create(e, stringValues.Count <= 1 ? stringValues.ToString() : stringValues.ToArray(), type),
                _ => ObjectWrapper.Create(e, target, type)
            });
        });

        return services;
    }
}
