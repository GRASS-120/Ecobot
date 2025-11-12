using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Concrete;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
using Grid.BuildingSystem.Buildings.Types.Furnance;
using Inventory;
using UnityEngine;

namespace Bot.Programming.BotPrograms
{
    /// <summary>
    /// Фабрика программы: добыть уголь → добыть железо → отнести всё в печь → Idle.
    /// Требует, чтобы в проекте были ScriptableObject'ы с именами "CoalData" и "IronData"
    /// (или переименуй строки ниже на свои GUID/ключи для ваших нод поиска).
    /// </summary>
    public static class CoalIronToFurnaceProgram
    {
        public static ProgNodeBase Build()
        {
            // узлы
            var idle         = new ProgNodeStateIdle();

            var findCoal     = new ProgNodeFindOre("CoalData");
            var moveToCoal   = new ProgNodeMoveTo();
            var mineCoal     = new ProgNodeMineOre("5"); // добудь 5 угля

            var findIron     = new ProgNodeFindOre("IronData");
            var moveToIron   = new ProgNodeMoveTo();
            var mineIron     = new ProgNodeMineOre("5"); // добудь 5 железной руды

            var findFurnace  = new ProgNodeFindBuilding(typeof(BuildingFurnace).Name); // или "Building_FurnaceData" — как у тебя заведено
            var moveToFurn   = new ProgNodeMoveTo();
            var putAll       = new ProgNodePut();

            // STREAM
            idle.Slots[0].Connect(findCoal);
            findCoal.Slots[0].Connect(moveToCoal);
            moveToCoal.Slots[0].Connect(mineCoal);

            mineCoal.Slots[0].Connect(findIron);
            findIron.Slots[0].Connect(moveToIron);
            moveToIron.Slots[0].Connect(mineIron);

            mineIron.Slots[0].Connect(findFurnace);
            findFurnace.Slots[0].Connect(moveToFurn);
            moveToFurn.Slots[0].Connect(putAll);
            putAll.Slots[0].Connect(idle); // цикл

            // DATA
            // уголь
            var coalFound    = FindDataOut<environment.Ore.Ore>(findCoal, "Found Ore");
            FindDataIn<object>(moveToCoal, "Target")?.ConnectToDataSlot(coalFound);
            FindDataIn<environment.Ore.Ore>(mineCoal, "Target Ore")?.ConnectToDataSlot(coalFound);

            // железо
            var ironFound    = FindDataOut<environment.Ore.Ore>(findIron, "Found Ore");
            FindDataIn<object>(moveToIron, "Target")?.ConnectToDataSlot(ironFound);
            FindDataIn<environment.Ore.Ore>(mineIron, "Target Ore")?.ConnectToDataSlot(ironFound);

            // печь
            var furnFound    = FindDataOut<BuildingBase>(findFurnace, "Found Building");
            FindDataIn<object>(moveToFurn, "Target")?.ConnectToDataSlot(furnFound);
            FindDataIn<object>(putAll, "Target")?.ConnectToDataSlot(furnFound);

            return idle;
        }

        private static ProgNodeDataSlot<T> FindDataOut<T>(ProgNodeBase node, string name)
        {
            foreach (var s in node.Slots)
                if (s is ProgNodeDataSlot<T> d && (string.IsNullOrEmpty(name) || d.SlotName == name))
                    return d;
            return null;
        }

        private static ProgNodeDataSlot<T> FindDataIn<T>(ProgNodeBase node, string name)
        {
            foreach (var s in node.Slots)
                if (s is ProgNodeDataSlot<T> d && (string.IsNullOrEmpty(name) || d.SlotName == name))
                    return d;
            return null;
        }
    }
}
