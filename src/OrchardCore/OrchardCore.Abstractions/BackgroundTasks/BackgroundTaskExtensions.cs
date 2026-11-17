using System.Reflection;
using System.Text;

namespace OrchardCore.BackgroundTasks;

public static class BackgroundTaskExtensions
{
    private static readonly string[] _typeNameSuffixes = ["BackgroundTask", "Task"];

    public static BackgroundTaskSettings GetDefaultSettings(this IBackgroundTask task)
    {
        var technicalName = task.GetTaskName();

        var attribute = task.GetType().GetCustomAttribute<BackgroundTaskAttribute>();
        if (attribute != null)
        {
            return new BackgroundTaskSettings
            {
                Name = technicalName,
                Title = !string.IsNullOrWhiteSpace(attribute.Title) ? attribute.Title : GetTitleFromTypeName(task.GetType().Name),
                Enable = attribute.Enable,
                Schedule = attribute.Schedule,
                Description = attribute.Description,
                LockTimeout = attribute.LockTimeout,
                LockExpiration = attribute.LockExpiration,
                UsePipeline = attribute.UsePipeline,
            };
        }

        return new BackgroundTaskSettings()
        {
            Name = technicalName,
            Title = GetTitleFromTypeName(task.GetType().Name),
        };
    }

    public static IBackgroundTask GetTaskByName(this IEnumerable<IBackgroundTask> tasks, string name)
        => tasks.LastOrDefault(task => task.GetTaskName() == name);

    public static string GetTaskName(this IBackgroundTask task) => task.GetType().FullName;

    /// <summary>
    /// Builds a readable title from the name of a task type that doesn't define one, for instance
    /// 'Chunk File Upload' from 'ChunkFileUploadBackgroundTask'.
    /// </summary>
    internal static string GetTitleFromTypeName(string typeName)
    {
        var name = typeName;
        foreach (var suffix in _typeNameSuffixes)
        {
            if (name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal))
            {
                name = name[..^suffix.Length];
                break;
            }
        }

        var builder = new StringBuilder(name.Length + 8);
        for (var i = 0; i < name.Length; i++)
        {
            var current = name[i];

            // Insert a space before an upper case letter starting a word, keeping acronyms together.
            if (i > 0 && char.IsUpper(current) &&
                (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1]))))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
