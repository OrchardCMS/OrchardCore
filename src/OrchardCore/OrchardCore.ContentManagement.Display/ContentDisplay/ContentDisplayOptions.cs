using System.Collections.Frozen;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

public class ContentDisplayOptions
{
    private readonly List<ContentPartDisplayOption> _contentParts = [];
    private readonly List<ContentFieldDisplayOption> _contentFields = [];

    private FrozenDictionary<string, ContentPartDisplayOption> _contentPartOptions;
    private FrozenDictionary<string, ContentFieldDisplayOption> _contentFieldOptions;

    public IReadOnlyDictionary<string, ContentPartDisplayOption> ContentPartOptions
        => _contentPartOptions ??= _contentParts.ToFrozenDictionary(k => k.Type.Name);

    public IReadOnlyDictionary<string, ContentFieldDisplayOption> ContentFieldOptions
        => _contentFieldOptions ??= _contentFields.ToFrozenDictionary(k => k.Type.Name);

    internal void ForContentPartDisplayMode(Type contentPartType, Type displayDriverType, Func<string, bool> predicate)
    {
        if (!typeof(IContentPartDisplayDriver).IsAssignableFrom(displayDriverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentPartDisplayDriver));
        }

        var option = GetOrAddContentPartDisplayOption(contentPartType);
        option.ForDisplayMode(displayDriverType, predicate);
    }

    internal void ForContentPartEditor(Type contentPartType, Type editorDriverType, Func<string, bool> predicate)
    {
        if (!typeof(IContentPartDisplayDriver).IsAssignableFrom(editorDriverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentPartDisplayDriver));
        }

        var option = GetOrAddContentPartDisplayOption(contentPartType);
        option.ForEditor(editorDriverType, predicate);
    }

    internal void RemoveContentPartDisplayDriver(Type contentPartType, Type driverType)
    {
        if (!typeof(IContentPartDisplayDriver).IsAssignableFrom(driverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentPartDisplayDriver));
        }

        var option = GetOrAddContentPartDisplayOption(contentPartType);
        option.RemoveDisplayDriver(driverType);
    }

    private ContentPartDisplayOption GetOrAddContentPartDisplayOption(Type contentPartType)
    {
        if (!contentPartType.IsSubclassOf(typeof(ContentPart)))
        {
            throw new ArgumentException("The type must inherit from " + nameof(ContentPart));
        }

        var option = _contentParts.FirstOrDefault(x => x.Type == contentPartType);
        if (option == null)
        {
            option = new ContentPartDisplayOption(contentPartType);
            _contentParts.Add(option);
        }

        return option;
    }

    internal void ForContentFieldDisplayMode(Type contentFieldType, Type displayModeDriverType, Func<string, bool> predicate)
    {
        if (!typeof(IContentFieldDisplayDriver).IsAssignableFrom(displayModeDriverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentFieldDisplayDriver));
        }

        var option = GetOrAddContentFieldDisplayOption(contentFieldType);
        option.ForDisplayMode(displayModeDriverType, predicate);
    }

    internal void ForContentFieldEditor(Type contentFieldType, Type editorDriverType, Func<string, bool> predicate)
    {
        if (!typeof(IContentFieldDisplayDriver).IsAssignableFrom(editorDriverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentFieldDisplayDriver));
        }

        var option = GetOrAddContentFieldDisplayOption(contentFieldType);
        option.ForEditor(editorDriverType, predicate);
    }

    internal void RemoveContentFieldDisplayDriver(Type contentPartType, Type driverType)
    {
        if (!typeof(IContentFieldDisplayDriver).IsAssignableFrom(driverType))
        {
            throw new ArgumentException("The type must implement " + nameof(IContentFieldDisplayDriver));
        }

        var option = GetOrAddContentFieldDisplayOption(contentPartType);
        option.RemoveDisplayDriver(driverType);
    }

    private ContentFieldDisplayOption GetOrAddContentFieldDisplayOption(Type contentFieldType)
    {
        if (!contentFieldType.IsSubclassOf(typeof(ContentField)))
        {
            throw new ArgumentException("The type must inherit from " + nameof(ContentField));
        }

        var option = _contentFields.FirstOrDefault(x => x.Type == contentFieldType);
        if (option == null)
        {
            option = new ContentFieldDisplayOption(contentFieldType);
            _contentFields.Add(option);
        }

        return option;
    }
}
