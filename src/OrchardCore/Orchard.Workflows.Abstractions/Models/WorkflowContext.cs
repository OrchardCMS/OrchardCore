using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;

namespace Orchard.Workflows.Models
{
    public class WorkflowContext
    {
        private dynamic _workflowState;
        private Workflow _workflow;

        /// <summary>
        /// If set, represents the subject of the current workflow
        /// </summary>
        public IContent Content { get; set; }

        public IDictionary<string, object> Tokens { get; set; }

        private dynamic State
        {
            get { return _workflowState ?? (_workflowState = FormParametersHelper.FromJsonString(Record.State)); }
        }

        public Workflow Record
        {
            get { return _workflow; }
            set
            {
                _workflow = value;
                _workflowState = null;
            }
        }

        public void SetState<T>(string key, T value)
        {
            State[key] = JToken.FromObject(value);
            SerializeState();
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

        private void SerializeState()
        {
            Record.State = FormParametersHelper.ToJsonString(State);
        }

        private string KeyFor(Activity record, string key)
        {
            return "@" + record.Id + "_" + key;
        }

        public IEnumerable<Transition> GetInboundTransitions(Activity activityRecord)
        {
            return _workflow.Definition
                .Transitions
                .Where(transition =>
                    transition.DestinationActivity == activityRecord
                ).ToArray();
        }

        public IEnumerable<Transition> GetOutboundTransitions(Activity activityRecord)
        {
            return _workflow.Definition
                .Transitions
                .Where(transition =>
                    transition.SourceActivity == activityRecord
                ).ToArray();
        }

        public IEnumerable<Transition> GetOutboundTransitions(Activity activityRecord, LocalizedString outcome)
        {
            return _workflow.Definition
                .Transitions
                .Where(transition =>
                    transition.SourceActivity == activityRecord
                    && transition.SourceEndpoint == outcome.TextHint
                ).ToArray();
        }

    }
}