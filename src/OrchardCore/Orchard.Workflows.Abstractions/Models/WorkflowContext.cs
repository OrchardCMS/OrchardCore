using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;

namespace Orchard.Workflows.Models
{
    public class WorkflowContext
    {

        //private dynamic _workflowState;
        //private WorkflowRecord _workflowRecord;

        ///// <summary>
        ///// If set, represents the subject of the current workflow
        ///// </summary>
        //public IContent Content { get; set; }

        //public IDictionary<string, object> Tokens { get; set; }

        //private dynamic State
        //{
        //    get { return _workflowState ?? (_workflowState = FormParametersHelper.FromJsonString(Record.State)); }
        //}

        //public WorkflowRecord Record
        //{
        //    get { return _workflowRecord; }
        //    set
        //    {
        //        _workflowRecord = value;
        //        _workflowState = null;
        //    }
        //}

        //public void SetState<T>(string key, T value)
        //{
        //    State[key] = JToken.FromObject(value);
        //    SerializeState();
        //}

        //public T GetState<T>(string key)
        //{
        //    if (State == null)
        //    {
        //        return default(T);
        //    }

        //    var value = State[key];
        //    return value != null ? value.ToObject<T>() : default(T);
        //}

        //public object GetState(string key)
        //{
        //    if (State == null)
        //    {
        //        return null;
        //    }

        //    return State[key];
        //}

        //public void SetStateFor<T>(ActivityRecord record, string key, T value)
        //{
        //    SetState(KeyFor(record, key), value);
        //}

        //public bool HasStateFor(ActivityRecord record, string key)
        //{
        //    return GetState(KeyFor(record, key)) != null;
        //}

        //public T GetStateFor<T>(ActivityRecord record, string key)
        //{
        //    return GetState<T>(KeyFor(record, key));
        //}

        //public object GetStateFor(ActivityRecord record, string key)
        //{
        //    return GetStateFor<object>(record, key);
        //}

        //private void SerializeState()
        //{
        //    Record.State = FormParametersHelper.ToJsonString(State);
        //}

        //private string KeyFor(ActivityRecord record, string key)
        //{
        //    return "@" + record.Id + "_" + key;
        //}

        //public IEnumerable<TransitionRecord> GetInboundTransitions(ActivityRecord activityRecord)
        //{
        //    return _workflowRecord.WorkflowDefinitionRecord
        //        .TransitionRecords
        //        .Where(transition =>
        //            transition.DestinationActivityRecord == activityRecord
        //        ).ToArray();
        //}

        //public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord)
        //{
        //    return _workflowRecord.WorkflowDefinitionRecord
        //        .TransitionRecords
        //        .Where(transition =>
        //            transition.SourceActivityRecord == activityRecord
        //        ).ToArray();
        //}

        //public IEnumerable<TransitionRecord> GetOutboundTransitions(ActivityRecord activityRecord, LocalizedString outcome)
        //{
        //    return _workflowRecord.WorkflowDefinitionRecord
        //        .TransitionRecords
        //        .Where(transition =>
        //            transition.SourceActivityRecord == activityRecord
        //            && transition.SourceEndpoint == outcome.TextHint
        //        ).ToArray();
        //}

    }
}