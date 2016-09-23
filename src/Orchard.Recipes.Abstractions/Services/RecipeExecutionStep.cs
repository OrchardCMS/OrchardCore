using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public abstract class RecipeExecutionStep : IRecipeExecutionStep
    {
        private readonly ILogger _logger;
        protected readonly IStringLocalizer T;

        public RecipeExecutionStep(ILoggerFactory loggerFactory,
            IStringLocalizer stringLocalizer)
        {
            _logger = loggerFactory.CreateLogger(GetType().Name);

            T = stringLocalizer;
        }

        public abstract string Name { get; }

        public virtual IEnumerable<string> Names
        {
            get { yield return Name; }
        }

        public virtual LocalizedString DisplayName
        {
            get { return T[Name]; }
        }

        public virtual LocalizedString Description
        {
            get { return DisplayName; }
        }

        protected virtual string Prefix
        {
            get { return GetType().Name; }
        }

        protected virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual void Configure(RecipeExecutionStepConfigurationContext context)
        {
        }

        public virtual void UpdateStep(UpdateRecipeExecutionStepContext context)
        {
        }

        public abstract Task ExecuteAsync(RecipeExecutionContext context);
    }
}