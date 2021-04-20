using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Filters.Abstractions.Nodes;

namespace OrchardCore.Filters.Abstractions.Services
{
    public abstract class FilterResult<T, TTermOption> : IEnumerable<TermNode> where TTermOption : TermOption
    {
        protected IReadOnlyDictionary<string, TTermOption> _termOptions;

        protected Dictionary<string, TermNode> _terms = new Dictionary<string, TermNode>();

        public FilterResult(IReadOnlyDictionary<string, TTermOption> termOptions)
        {
            _termOptions = termOptions;
        }

        public FilterResult(List<TermNode> terms, IReadOnlyDictionary<string, TTermOption> termOptions)
        {
            _termOptions = termOptions;

            foreach (var term in terms)
            {
                TryAddOrReplace(term);
            }
        }

        public void MapTo<TModel>(TModel model)
        {
            foreach (var term in _terms.Values)
            {
                var option = _termOptions[term.TermName];

                if (option.MapTo is Action<string, TModel> action &&
                    term is TermOperationNode operationNode &&
                    operationNode.Operation is UnaryNode node)
                {
                    action(node.Value, model);
                }
            }
        }

        public string ToNormalizedString()
            => $"{String.Join(" ", _terms.Values.Select(s => s.ToNormalizedString()))}";

        public override string ToString()
            => $"{String.Join(" ", _terms.Values.Select(s => s.ToString()))}";

        public bool TryAddOrReplace(TermNode term)
        {
            // Check the term options
            if (!_termOptions.TryGetValue(term.TermName, out var termOption))
            {
                return false;
            }

            if (_terms.TryGetValue(term.TermName, out var existingTerm))
            {
                if (termOption.Single)
                {
                    // Replace
                    _terms[term.TermName] = term;
                    return true;
                }

                // Add
                if (existingTerm is CompoundTermNode compound)
                {
                    compound.Children.Add(term as TermOperationNode);
                }
                else
                {
                    // TODO this isn't going to work when removing from list,
                    // i.e. search says tax:a tax:b but model says just tax:b
                    // for that we need a Merge extension.
                    var newCompound = new AndTermNode(existingTerm as TermOperationNode, term as TermOperationNode);
                    _terms[term.TermName] = newCompound;
                    return true;
                }
            }

            _terms[term.TermName] = term;

            return true;
        }

        public bool TryRemove(string name)
            => _terms.Remove(name);

        public IEnumerator<TermNode> GetEnumerator()
            => _terms.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _terms.Values.GetEnumerator();
    }
}
