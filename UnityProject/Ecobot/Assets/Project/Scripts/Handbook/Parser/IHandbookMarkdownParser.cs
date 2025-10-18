namespace Handbook.Parser
{
    public interface IHandbookMarkdownParser
    {
        HandbookParseResult Parse(string pageId, string rawMarkdown);
    }
}