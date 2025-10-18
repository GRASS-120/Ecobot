using System.Collections.Generic;
using Handbook.Models;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;
using Handbook.Routing;
using Handbook.Routing.Actions;

namespace Handbook.Parser
{
    public class LinkCollector
    {
        private readonly IHandbookLinkRouter _router;

    public LinkCollector(IHandbookLinkRouter router)
    {
        _router = router;
    }

    public List<HandbookLink> Collect(IReadOnlyList<HandbookBlockBase> blocks)
    {
        var list = new List<HandbookLink>();
        if (blocks == null || blocks.Count == 0)
            return list;

        for (int i = 0; i < blocks.Count; i++)
            VisitBlock(blocks[i], list);

        return list;
    }

    private void VisitBlock(HandbookBlockBase block, List<HandbookLink> sink)
    {
        switch (block)
        {
            case HeadingBlock h:
                VisitInlines(h.inlines, sink);
                break;
            case ParagraphBlock p:
                VisitInlines(p.inlines, sink);
                break;
            case ListBlock l:
                for (int i = 0; i < l.items.Count; i++)
                    VisitListItem(l.items[i], sink);
                break;
            case QuoteBlock q:
                for (int i = 0; i < q.children.Count; i++)
                    VisitBlock(q.children[i], sink);
                break;
        }
    }

    private void VisitListItem(ListItemBlock item, List<HandbookLink> sink)
    {
        for (int i = 0; i < item.children.Count; i++)
            VisitBlock(item.children[i], sink);
    }

    private void VisitInlines(List<HandbookInlineBase> inlines, List<HandbookLink> sink)
    {
        if (inlines == null) return;

        for (int i = 0; i < inlines.Count; i++)
        {
            if (inlines[i] is LinkInline li)
            {
                var action = _router.Resolve(li.url);
                var link = MapActionToLink(li.url, action);
                sink.Add(link);
            }
        }
    }

    private HandbookLink MapActionToLink(string url, LinkActionBase action)
    {
        var link = new HandbookLink { url = url };

        // Отображаем типизированное действие в модель ссылки без вложенных switch
        if (action is OpenHandbookPageAction open)
        {
            link.kind = "handbook";
            link.pageId = open.PageId;
            link.anchor = open.Anchor;
            return link;
        }

        if (action is TriggerTutorialStepAction step)
        {
            link.kind = "tutorial";
            link.stepId = step.StepId;
            return link;
        }

        if (action is OpenExternalUrlAction)
        {
            link.kind = "external";
            return link;
        }

        link.kind = "invalid";
        return link;
    }
    }
}