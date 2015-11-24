using System.Collections.Generic;
using Orchard.Localization;
using Orchard.DependencyInjection;
using Microsoft.Extensions.Localization;
using Orchard.Environment.Recipes.Models;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;

namespace Orchard.Environment.Recipes.Services
{
    public abstract class RecipeExecutionStep : IDependency, IRecipeExecutionStep
    {
        private readonly ILogger _logger;

        public RecipeExecutionStep(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType().Name);
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public abstract string Name { get; }

        public virtual IEnumerable<string> Names
        {
            get { yield return Name; }
        }

        public virtual LocalizedString DisplayName
        {
            get { return T(Name); }
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

        public virtual dynamic BuildEditor(dynamic shapeFactory)
        {
            return null;
        }

        public virtual dynamic UpdateEditor(dynamic shapeFactory, IUpdateModel updater)
        {
            return null;
        }

        public virtual void Configure(RecipeExecutionStepConfigurationContext context)
        {
        }

        public virtual void UpdateStep(UpdateRecipeExecutionStepContext context)
        {
        }

        public abstract void Execute(RecipeExecutionContext context);
    }
}