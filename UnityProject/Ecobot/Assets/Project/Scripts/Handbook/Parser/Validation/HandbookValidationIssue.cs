namespace Handbook.Parser.Validation
{
    public class HandbookValidationIssue
    {
        public HandbookValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public string PageId { get; }
        public string Anchor { get; }
        public string LinkUrl { get; }
        public string StepId { get; }

        public HandbookValidationIssue(
            HandbookValidationSeverity severity,
            string code,
            string message,
            string pageId = null,
            string anchor = null,
            string linkUrl = null,
            string stepId = null)
        {
            Severity = severity;
            Code = code ?? "HB000";
            Message = message ?? string.Empty;
            PageId = pageId;
            Anchor = anchor;
            LinkUrl = linkUrl;
            StepId = stepId;
        }
    }
}