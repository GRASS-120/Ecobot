using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bot.Programming.Nodes.Base
{
    public abstract class ProgNodeBase
    {
        public string NodeName { get; protected set; }
        public string Description { get; protected set; }
        
        protected List<ProgNodeSlotBase> slots = new ();
        
        public List<ProgNodeSlotBase> Slots => slots;
        
        // Выполнение ноды - возвращает следующую ноду для выполнения
        public abstract IEnumerator Execute(BotBase bot, BotProgramExecutor executor);
    }
}