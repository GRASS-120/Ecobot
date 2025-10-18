namespace Handbook.Routing
{
    public sealed class TriggerTutorialStepAction : LinkActionBase
    {
        public string StepId { get; }

        public TriggerTutorialStepAction(string raw, string stepId)
        {
            Raw = raw;
            StepId = stepId;
        }
    }
}