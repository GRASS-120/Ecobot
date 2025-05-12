using System.Collections;
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    public class ProgNodeStateIdle : ProgNodeState
    {
        public ProgNodeStateIdle() : base("Idle")
        {
            Description = "Bot is idle and waiting for commands";
        }
        
        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            Debug.Log($"[{NodeName}] Bot is now idle");
            
            if (nextSlot.ConnectedNode != null)
            {
                yield return executor.ExecuteNode(nextSlot.ConnectedNode);
            }
            else
            {
                Debug.Log($"[{NodeName}] End of program (no next node)");
            }
        }
    }
}