using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Redis.Services
{
    public class RedisTagCache : ITagCache
    {
        private readonly IRedisService _redis;
        private readonly string _prefix;
        private readonly IEnumerable<ITagRemovedEventHandler> _tagRemovedEventHandlers;
        private readonly ILogger _logger;

        public RedisTagCache(
            IRedisService redis,
            ShellSettings shellSettings,
            IEnumerable<ITagRemovedEventHandler> tagRemovedEventHandlers,
            ILogger<RedisTagCache> logger)
        {
            _redis = redis;
            _prefix = redis.InstancePrefix + shellSettings.Name + ":Tag:";
            _tagRemovedEventHandlers = tagRemovedEventHandlers;
            _logger = logger;
        }

        public async Task TagAsync(string key, params string[] tags)
        {
            if (_redis.Database == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Database == null)
                {
                    _logger.LogError("Fails to add the '{KeyName}' to the {PrefixName} tags.", key, _prefix);
                    return;
                }
            }

            try
            {
                foreach (var tag in tags)
                {
                    await _redis.Database.SetAddAsync(_prefix + tag, key);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to add the '{KeyName}' to the {PrefixName} tags.", key, _prefix);
            }
        }

        public async Task<IEnumerable<string>> GetTaggedItemsAsync(string tag)
        {
            if (_redis.Database == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Database == null)
                {
                    _logger.LogError("Fails to get '{TagName}' items.", _prefix + tag);
                    return Enumerable.Empty<string>();
                }
            }

            try
            {
                var values = await _redis.Database.SetMembersAsync(_prefix + tag);

                if (values == null || values.Length == 0)
                {
                    return Enumerable.Empty<string>();
                }

                return values.Select(v => (string)v).ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to get '{TagName}' items.", _prefix + tag);
            }

            return Enumerable.Empty<string>();
        }

        public async Task RemoveTagAsync(string tag)
        {
            if (_redis.Database == null)
            {
                await _redis.ConnectAsync();
                if (_redis.Database == null)
                {
                    _logger.LogError("Fails to remove the '{TagName}'.", _prefix + tag);
                    return;
                }
            }

            try
            {
                var values = await _redis.Database.SetMembersAsync(_prefix + tag);

                if (values == null || values.Length == 0)
                {
                    return;
                }

                var set = values.Select(v => (string)v).ToArray();

                await _redis.Database.KeyDeleteAsync(_prefix + tag);

                await _tagRemovedEventHandlers.InvokeAsync(x => x.TagRemovedAsync(tag, set), _logger);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Fails to remove the '{TagName}'.", _prefix + tag);
            }
        }
    }
}
