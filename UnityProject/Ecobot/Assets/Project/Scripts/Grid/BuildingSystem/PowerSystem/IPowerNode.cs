using System.Collections.Generic;
using Grid.BuildingSystem.Buildings;

namespace Grid.BuildingSystem.PowerSystem
{
    public interface IPowerNode
    {
        PowerNodeType NodeType { get; }
        int ProducedUnits { get; }
        int ConsumedUnits { get; }
        int MaxInputs { get; }
        int MaxOutputs { get; }
        bool IsBroken { get; }
        BuildingBase Building { get; }

        IReadOnlyList<IPowerNode> Inputs { get; }
        IReadOnlyList<IPowerNode> Outputs { get; }

        // Ограничения по ролям и портам
        bool CanAcceptInputFrom(IPowerNode from);
        bool CanProvideOutputTo(IPowerNode to);

        // Фактическое изменение графа (локально для узла)
        bool TryConnectInput(IPowerNode from);
        bool TryConnectOutput(IPowerNode to);
        void Disconnect(IPowerNode other);

        // Сервис сообщает узлу состояние питания (для потребителей)
        void OnPowerStateChanged(bool isPowered);

        // Поломка/ремонт (для генераторов)
        void MarkBroken();
        void Repair();
    }
}