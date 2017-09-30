using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;

namespace Orchard.Workflows.Models
{
    public class WorkflowContext
    {
        /// <summary>
        /// If set, represents the subject of the current workflow
        /// </summary>
        public IContent Content { get; set; }

        public IDictionary<string, object> Tokens { get; set; }

        public dynamic State { get; private set; }

        public Workflow Workflow { get; set; }

        public void SetState<T>(string key, T value)
        {
            State[key] = JToken.FromObject(value);
        }

        public T GetState<T>(string key)
        {
            if (State == null)
            {
                return default(T);
            }

            var value = State[key];
            return value != null ? value.ToObject<T>() : default(T);
        }

        public object GetState(string key)
        {
            if (State == null)
            {
                return null;
            }

            return State[key];
        }

        public void SetStateFor<T>(Activity record, string key, T value)
        {
            SetState(KeyFor(record, key), value);
        }

        public bool HasStateFor(Activity record, string key)
        {
            return GetState(KeyFor(record, key)) != null;
        }

        public T GetStateFor<T>(Activity record, string key)
        {
            return GetState<T>(KeyFor(record, key));
        }

        public object GetStateFor(Activity record, string key)
        {
            return GetStateFor<object>(record, key);
        }

        private string KeyFor(Activity record, string key)
        {
            return "@" + record.Id + "_" + key;
        }

        public IEnumerable<Transition> GetInboundTransitions(Activity activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Transition> GetOutboundTransitions(Activity activityRecord)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Transition> GetOutboundTransitions(Activity activityRecord, LocalizedString outcome)
        {
            throw new System.NotImplementedException();
        }
    }
}