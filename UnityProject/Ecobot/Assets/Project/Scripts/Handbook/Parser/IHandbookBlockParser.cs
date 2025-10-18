namespace Handbook.Parser
{
    public interface IHandbookBlockParser
    {
        bool CanParse(LineCursor cursor);
        HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context);
    }
}