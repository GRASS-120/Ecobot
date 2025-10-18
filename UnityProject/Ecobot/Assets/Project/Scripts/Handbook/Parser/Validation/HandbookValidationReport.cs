using System.Collections.Generic;

namespace Handbook.Parser.Validation
{
    public class HandbookValidationReport
    {
        public IReadOnlyList<HandbookValidationIssue> Issues => _issues;

        private readonly List<HandbookValidationIssue> _issues = new();

        public void Add(HandbookValidationIssue issue)
        {
            if (issue != null) _issues.Add(issue);
        }

        public void AddRange(IEnumerable<HandbookValidationIssue> issues)
        {
            if (issues == null) return;
            _issues.AddRange(issues);
        }

        public bool HasErrors()
        {
            for (int i = 0; i < _issues.Count; i++)
                if (_issues[i].Severity == HandbookValidationSeverity.Error)
                    return true;
            return false;
        }

        public IEnumerable<HandbookValidationIssue> GetBySeverity(HandbookValidationSeverity severity)
        {
            for (int i = 0; i < _issues.Count; i++)
                if (_issues[i].Severity == severity)
                    yield return _issues[i];
        }
    }
}