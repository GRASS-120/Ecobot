using System.Collections;
using System.Linq;
using Bot.Programming.Nodes.Slots;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeFindOre : ProgNodeAction
    {
        private readonly string oreTypeName;
        private readonly ProgNodeDataSlot<environment.Ore.Ore> foundOreSlot;

        // --- простые такты/задержки ---
        private readonly float FoundDelaySec;
        private readonly float FailRetryDelaySec;

        public ProgNodeFindOre(string oreTypeName, float foundDelaySec = 0.10f, float failRetryDelaySec = 0.10f)
            : base("Find Ore")
        {
            this.oreTypeName = oreTypeName;
            this.FoundDelaySec = Mathf.Max(0f, foundDelaySec);
            this.FailRetryDelaySec = Mathf.Max(0f, failRetryDelaySec);

            Description = $"Find nearest ore of type '{oreTypeName}'";
            foundOreSlot = new ProgNodeDataSlot<environment.Ore.Ore>("Found Ore", this);
            slots.Add(foundOreSlot);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            if (bot == null) yield break;

            var allOres = GameObject.FindObjectsOfType<environment.Ore.Ore>();
            if (allOres == null || allOres.Length == 0)
            {
                if (FailRetryDelaySec > 0f) yield return new WaitForSeconds(FailRetryDelaySec);
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            // фильтруем по типу
            var matching = allOres.Where(o =>
            {
                if (o == null || !o.isActiveAndEnabled) return false;
                var tp = o.GetComponent<environment.Ore.IOreTypeProvider>();
                return tp != null && string.Equals(tp.TypeId, oreTypeName, System.StringComparison.Ordinal);
            }).ToList();

            if (matching.Count == 0)
            {
                if (FailRetryDelaySec > 0f) yield return new WaitForSeconds(FailRetryDelaySec);
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            // ближайшая
            var botPos = bot.transform.position;
            var closest = matching.OrderBy(o => (o.transform.position - botPos).sqrMagnitude).First();

            foundOreSlot.Value = closest;

            // --- такт после успешного поиска ---
            if (FoundDelaySec > 0f) yield return new WaitForSeconds(FoundDelaySec);

            if (successSlot?.ConnectedNode != null)
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
        }
    }
}
