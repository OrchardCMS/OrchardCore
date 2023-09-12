using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace OrchardCore.Modules.Manifest
{
    using Xunit;
    using Xunit.Abstractions;
    using static BindingFlags;

    // TODO: TBD: now to test I want to look at actually doing this in a demo-assembly if possible...
    // TODO: TBD: which will be a bit tricky considering ModuleAttribute can appear exactly once...
    // TODO: TBD: testing via the CSPROJ aspect... i.e. to load the assembly in reference and check that it 'works' ...
    /// <summary>
    /// Provides some <see cref="ModuleAttribute"/> tests.
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class FeatureAttributeTests<TAttribute>
        where TAttribute : FeatureAttribute
    {
        // TODO: TBD: in addition to ctors... should be able to still set properties in the same way...
        // TODO: TBD: in effect the ctors are 'sugar' after a sort that provide some shorthand

        /// <summary>
        /// Gets the OutputHelper for the tests.
        /// </summary>
        protected virtual ITestOutputHelper OutputHelper { get; }

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultName"/>.
        /// </summary>
        protected static string DefaultName => FeatureAttribute.DefaultName;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultCategory"/>.
        /// </summary>
        protected static string DefaultCategory => FeatureAttribute.DefaultCategory;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultPriority"/>.
        /// </summary>
        protected static int DefaultPriority => FeatureAttribute.DefaultPriority;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultDescription"/>.
        /// </summary>
        protected static string DefaultDescription => FeatureAttribute.DefaultDescription;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultFeatureDependencies"/>.
        /// </summary>
        protected static string DefaultFeatureDependencies => FeatureAttribute.DefaultFeatureDependencies;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultDefaultTenantOnly"/>.
        /// </summary>
        protected static bool DefaultDefaultTenantOnly => FeatureAttribute.DefaultDefaultTenantOnly;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.DefaultAlwaysEnabled"/>.
        /// </summary>
        protected static bool DefaultAlwaysEnabled => FeatureAttribute.DefaultAlwaysEnabled;

        /// <summary>
        /// Gets the <see cref="FeatureAttribute.ListDelims"/>.
        /// </summary>
        protected static IEnumerable<char> ListDelims => FeatureAttribute.ListDelims;

        /// <summary>
        /// Returns a <paramref name="count"/> of Lorem Ipsum Words.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        protected static string LoremWords(int count) => LoremNET.Lorem.Words(count);

        /// <summary>
        /// Public|Instance
        /// </summary>
        private const BindingFlags CtorFlags = Public | Instance;

        /// <summary>
        /// Creates a <typeparamref name="TAttribute"/> from its <paramref name="args"/>, with
        /// the ability to apply a <paramref name="classifier"/> to each of the arguments. We
        /// classify the <see cref="Type"/> of each argument this way because each one may be
        /// Null, particularly, <see cref="String"/> arguments, which is okay for Features,
        /// Modules, etc.
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        protected virtual TAttribute CreateFromArgs(Func<int, object, Type> classifier, params object[] args)
        {
            static Type ParameterlessCtorClassifier(int index, object arg) => typeof(object);

            if (!args.Any() || classifier == null)
            {
                classifier ??= ParameterlessCtorClassifier;
            }

            // Classify each Argument, not simply getting its type, because each may be Null, which is okay.
            var types = args.Select((arg, index) => classifier.Invoke(index, arg)).ToArray();

            var allAttributeCtors = typeof(TAttribute).GetConstructors(CtorFlags);

            // Identify the Ctor with the Parameters aligned to the Classified Arguments.
            var allAttributeCtorWithParameterTypes = allAttributeCtors.Select(ctor => new
            {
                Callback = ctor,
                Types = ctor.GetParameters().Select(_ => _.ParameterType).ToArray(),
            }).ToArray();

            var attributeCtor = allAttributeCtorWithParameterTypes.SingleOrDefault(_ => _.Types.SequenceEqual(types))
                ?.Callback
                ?? throw new ArgumentException($"Unable to align ctor to args({args.Length}).", nameof(args));

            var feature = attributeCtor.Invoke(args);

            return Assert.IsType<TAttribute>(feature);
        }

        /// <summary>
        /// Classifier supporting
        /// <see cref="FeatureAttribute(String, String, String, Boolean, Boolean, Boolean)"/>, arguments in
        /// order, <c>id, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type FeatureString3Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                3 or 4 or 5 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="FeatureAttribute(String, String, String, String, Boolean, Boolean, Boolean)"/>, arguments in
        /// order, <c>id, name, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type FeatureString4Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                4 or 5 or 6 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="FeatureAttribute(String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>, arguments in
        /// order, <c>id, name, category, priority, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type FeatureString6Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                6 or 7 or 8 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// When this function is invoked it is because we expect there to be a parameterless ctor
        /// which may be invoked.
        /// </summary>
        /// <returns></returns>
        private static TAttribute DefaultFactory()
        {
            var paramlessCtor = typeof(TAttribute).GetConstructor(CtorFlags, Array.Empty<Type>());
            Assert.NotNull(paramlessCtor);
            var instance = paramlessCtor.Invoke(Array.Empty<object>());
            var feature = Assert.IsType<TAttribute>(instance);
            return feature;
        }

        /// <summary>
        /// Creates a new <typeparamref name="TAttribute"/> Instance given the
        /// <paramref name="factory"/>. Defaults to <see cref="DefaultFactory"/> as the
        /// injected callback.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        protected virtual TAttribute CreateFromFactory(Func<TAttribute> factory = null) => (factory ?? DefaultFactory).Invoke();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        public FeatureAttributeTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected static IEnumerable<T> GetValues<T>(params T[] values)
        {
            foreach (var value in values)
            {
                yield return value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected static T[] GetArray<T>(params T[] values) => GetValues(values).ToArray();

        /// <summary>
        /// Provides a Pair that may be rendered to <see cref="String"/>.
        /// </summary>
        protected readonly struct RenderKeyValuePair
        {
            /// <summary>
            /// Pair instance supporting the Render.
            /// </summary>
            private readonly KeyValuePair<string, object> _pair;

            /// <summary>
            /// Gets the pair Key.
            /// </summary>
            public string Key => _pair.Key;

            /// <summary>
            /// Gets the pair Value.
            /// </summary>
            public object Value => _pair.Value;

            /// <summary>
            /// Default <see cref="Render"/> callback.
            /// </summary>
            /// <param name="_"></param>
            /// <returns></returns>
            /// <remarks>Given value may be Null, Nullable, Sring, or Boolean. Otherwise,
            /// makes a best effort to convert directly to string.</remarks>
            private static string DefaultRender(object _)
            {
                // Renders null as "null" as such.
                if (_ == null)
                {
                    return "null";
                }

                // Renders the Value as a string, with enclosing quotes.
                if (_ is string s)
                {
                    return $"\"{s}\"";
                }

                // Renders a Boolean as lowecase, i.e. true|false.
                if (_ is bool b)
                {
                    return $"{b}".ToLower();
                }

                // Otherwise makes a best effort to simply render using string interpolation.
                return $"{_}";
            }

            /// <summary>
            /// Gets the Render callback.
            /// </summary>
            private Func<object, string> Render { get; }

            /// <summary>
            /// Constructs an instance of the pair.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="render"></param>
            internal RenderKeyValuePair(string key, object value, Func<object, string> render = null)
            {
                _pair = new KeyValuePair<string, object>(key, value);
                Render = render ?? DefaultRender;
            }

            /// <summary>
            /// Deconstructs the instance, not including the <see cref="Render"/> callback..
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal void Deconstruct(out string key, out object value)
            {
                key = _pair.Key;
                value = _pair.Value;
            }

            /// <summary>
            /// Deconstructs the instance, including the <see cref="Render"/> callback.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="render"></param>
            [EditorBrowsable(EditorBrowsableState.Never)]
            internal void Deconstruct(out string key, out object value, out Func<object, string> render)
            {
                key = _pair.Key;
                value = _pair.Value;
                render = Render;
            }

            /// <inheritdoc/>
            public override string ToString() => String.Join(": ", $"\"{Key}\"", Render.Invoke(Value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        protected virtual string RenderKeyValuePairs(params RenderKeyValuePair[] pairs) =>
            String.Join(
                String.Join(", ", pairs.Select(_ => $"{_}")), "{}".ToCharArray().Select(_ => $"{_}")
            );

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pairs"></param>
        protected virtual void ReportKeyValuePairs(params RenderKeyValuePair[] pairs) =>
            OutputHelper.WriteLine($"{RenderKeyValuePairs(pairs)}");

        /// <summary>
        /// Override in order to enhance the Default coverage.
        /// </summary>
        /// <param name="feature"></param>
        protected virtual void VerifyDefault(TAttribute feature)
        {
            Assert.False(feature.Dependencies.Any());
            Assert.False(feature.DefaultTenantOnly);
            Assert.False(feature.IsAlwaysEnabled);
            Assert.False(feature.Exists);
            // Indeed the default-default Id is 'null' however in more of a context I think we would actually expect a value there
            Assert.Null(feature.Id);
            // Name, OTOH, different story, Id is the fallback response when Name is not provided
            Assert.Null(feature.Name);
            Assert.Empty(feature.Category);
            Assert.Equal(DefaultDescription, feature.Description);
            Assert.Equal($"{DefaultPriority}", feature.Priority);
        }

        private static bool DefaultAssemblyAttribPredicate<T>(T _) where T : Attribute => _ is not null;

        /// <summary>
        /// Returns the Attributes of type <typeparamref name="T"/> exposed by the
        /// <em>Assembly</em> anchored by the <paramref name="rootType"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootType"></param>
        /// <param name="predicate">Caller specified predicate, defaults to
        /// <see cref="DefaultAssemblyAttribPredicate{T}(T)"/>.</param>
        /// <returns></returns>
        protected virtual T[] GetAssemblyAttributes<T>(Type rootType, Func<T, bool> predicate = null)
            where T : Attribute
        {
            predicate ??= DefaultAssemblyAttribPredicate;
            var attribs = rootType.Assembly.GetCustomAttributes<T>().Where(predicate).ToArray();
            return attribs;
        }

        /// <summary>
        /// Returns the Attribute of type <typeparamref name="T"/> exposed by the
        /// <em>Assembly</em> anchored by the <paramref name="rootType"/>. Expecting there
        /// to be a single instance only aligned with the <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootType"></param>
        /// <param name="predicate">Caller specified predicate, defaults to <see cref="DefaultAssemblyAttribPredicate{T}(T)"/>.</param>
        /// <returns></returns>
        /// <remarks>Caller responsibility to vet on conditions such as Null or Not Null.
        /// Obviously, however, would throw when more than one is detected.</remarks>
        protected virtual T GetAssemblyAttribute<T>(Type rootType, Func<T, bool> predicate = null)
            where T : Attribute
        {
            // TODO: MWP: the danger in providing a predicate such as this, we potentially mask the issues such as ModuleMarker+Module...
            var attrib = GetAssemblyAttributes(rootType, predicate).SingleOrDefault();
            return attrib;
        }
    }
}
