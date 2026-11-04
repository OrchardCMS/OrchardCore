using NCrontab;

namespace OrchardCore.BackgroundTasks.Services;

internal sealed class BackgroundTaskManagementService
{
    private readonly IEnumerable<IBackgroundTask> _tasks;
    private readonly BackgroundTaskManager _manager;

    public BackgroundTaskManagementService(IEnumerable<IBackgroundTask> tasks, BackgroundTaskManager manager)
    {
        _tasks = tasks;
        _manager = manager;
    }

    public async Task<IReadOnlyList<BackgroundTaskSettings>> ListAsync()
    {
        var document = await _manager.GetDocumentAsync();
        return _tasks.Select(task => Effective(task, document.Settings.GetValueOrDefault(task.GetTaskName()))).ToArray();
    }

    public async Task<BackgroundTaskSettings> GetAsync(string name)
    {
        var task = _tasks.GetTaskByName(name);
        if (task is null) { return null; }
        var document = await _manager.GetDocumentAsync();
        return Effective(task, document.Settings.GetValueOrDefault(name));
    }

    public async Task<BackgroundTaskMutationResult> UpdateAsync(string name, BackgroundTaskConfiguration input)
    {
        var errors = Validate(input);
        if (errors.Count > 0) { return new() { Errors = errors }; }
        var settings = await GetAsync(name);
        if (settings is null) { return new() { Found = false }; }
        var changed = settings.Schedule != input.Schedule.Trim() || settings.Description != input.Description
            || settings.LockTimeout != input.LockTimeout || settings.LockExpiration != input.LockExpiration || settings.UsePipeline != input.UsePipeline;
        if (changed)
        {
            settings.Schedule = input.Schedule.Trim();
            settings.Description = input.Description;
            settings.LockTimeout = input.LockTimeout;
            settings.LockExpiration = input.LockExpiration;
            settings.UsePipeline = input.UsePipeline;
            await _manager.UpdateAsync(name, settings);
        }
        return new() { Changed = changed, Settings = settings };
    }

    public async Task<BackgroundTaskMutationResult> SetStatusAsync(string name, bool enabled)
    {
        var settings = await GetAsync(name);
        if (settings is null) { return new() { Found = false }; }
        if (enabled)
        {
            var errors = Validate(Configuration(settings));
            if (errors.Count > 0) { return new() { Errors = errors }; }
        }
        var changed = settings.Enable != enabled;
        if (changed)
        {
            settings.Enable = enabled;
            await _manager.UpdateAsync(name, settings);
        }
        return new() { Changed = changed, Settings = settings };
    }

    public static Dictionary<string, string[]> Validate(BackgroundTaskConfiguration input)
    {
        var errors = new Dictionary<string, string[]>();
        if (input is null)
        {
            errors["body"] = ["Task settings are required."];
            return errors;
        }
        if (string.IsNullOrWhiteSpace(input.Schedule) || input.Schedule.Length > 256 || CrontabSchedule.TryParse(input.Schedule.Trim()) is null)
        {
            errors["schedule"] = ["Provide a valid five-field cron expression."];
        }
        if (input.LockTimeout < 0) { errors["lockTimeout"] = ["Lock timeout must be nonnegative."]; }
        if (input.LockExpiration < 0) { errors["lockExpiration"] = ["Lock expiration must be nonnegative."]; }
        return errors;
    }

    public static BackgroundTaskConfiguration Configuration(BackgroundTaskSettings settings) => new()
    {
        Schedule = settings.Schedule, Description = settings.Description, LockTimeout = settings.LockTimeout,
        LockExpiration = settings.LockExpiration, UsePipeline = settings.UsePipeline,
    };

    private static BackgroundTaskSettings Effective(IBackgroundTask task, BackgroundTaskSettings stored)
    {
        var defaults = task.GetDefaultSettings();
        var settings = stored ?? defaults;
        return new()
        {
            Name = defaults.Name, Title = defaults.Title, Enable = settings.Enable,
            Schedule = settings.Schedule, Description = settings.Description, LockTimeout = settings.LockTimeout,
            LockExpiration = settings.LockExpiration, UsePipeline = settings.UsePipeline,
        };
    }
}

internal sealed class BackgroundTaskMutationResult
{
    public bool Found { get; init; } = true;
    public bool Changed { get; init; }
    public BackgroundTaskSettings Settings { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}

/// <summary>Complete writable settings for a registered tenant background task.</summary>
public sealed class BackgroundTaskConfiguration
{
    /// <summary>Gets or sets a five-field cron schedule, interpreted by the existing tenant scheduler.</summary>
    public string Schedule { get; set; }
    /// <summary>Gets or sets the optional administrative description.</summary>
    public string Description { get; set; }
    /// <summary>Gets or sets the nonnegative lock acquisition timeout in milliseconds; zero disables atomic locking.</summary>
    public int LockTimeout { get; set; }
    /// <summary>Gets or sets the nonnegative lock expiration in milliseconds; zero disables atomic locking.</summary>
    public int LockExpiration { get; set; }
    /// <summary>Gets or sets whether execution initializes and runs the tenant request pipeline.</summary>
    public bool UsePipeline { get; set; }
}
