using System;
using System.Collections;
using Bot;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Slots;
using environment.Ore;
using InteractionSystem;
using Inventory.LootSystem;
using R3;                 // IDisposable, Subscribe
using UnityEngine;

namespace Bot.Programming.Nodes.Concrete
{
    /// <summary>
    /// Mine Ore
    /// Входы:
    ///   - stream
    ///   - data "Target Ore" (environment.Ore.Ore)
    ///
    /// Выходы:
    ///   - stream: success / fail (из ProgNodeAction)
    ///   - data: "Mined Ore" (та же Ore)
    ///
    /// Параметры:
    ///   - desiredAmountText (строка): сколько единиц добыть (парсится в целое, >=1)
    /// </summary>
    public class ProgNodeMineOre : ProgNodeAction
    {
        private readonly ProgNodeDataSlot<Ore> _oreInput;
        private readonly ProgNodeDataSlot<Ore> _oreOutput;

        private readonly string _desiredAmountText;

        public ProgNodeMineOre(string desiredAmountText = "1") : base("Mine Ore")
        {
            _desiredAmountText = string.IsNullOrWhiteSpace(desiredAmountText) ? "1" : desiredAmountText;

            Description = "Mine passed ore until desired amount or depletion";

            _oreInput = new ProgNodeDataSlot<Ore>("Target Ore", this);
            slots.Add(_oreInput);

            _oreOutput = new ProgNodeDataSlot<Ore>("Mined Ore", this);
            slots.Add(_oreOutput);
        }

        public override IEnumerator Execute(BotBase bot, BotProgramExecutor executor)
        {
            int desired = ParseDesiredAmount(_desiredAmountText);
            if (desired <= 0) desired = 1;

            var ore = _oreInput.Value;
            if (ore == null)
            {
                Debug.Log("[ProgNodeMineOre] Target Ore is NULL → fail");
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            BotInteractor interactor = bot != null ? bot.Interactor : null;
            if (interactor == null)
            {
                Debug.LogWarning("[ProgNodeMineOre] Bot has no BotInteractor → fail");
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            // ⬇️ Новое поведение: если руда вне радиуса — пропускаем майнинг и идём по Success
            if (!interactor.IsTargetInRange(ore.transform))
            {
                Debug.Log("[ProgNodeMineOre] Ore is out of range → skip mining and continue (Success)");
                if (successSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(successSlot.ConnectedNode);
                yield break;
            }

            var lootProvider = ore as ILootProvider;

            int mined = 0;
            IDisposable lootSub = null;
            if (lootProvider != null)
            {
                lootSub = lootProvider.OnProvideLoot.Subscribe(q =>
                {
                    int gained = (q != null && q.Amount > 0) ? q.Amount : 1;
                    mined += gained;
                });
            }

            var oreInteractable = ore as IInteractable;
            if (oreInteractable == null)
            {
                Debug.Log("[ProgNodeMineOre] Ore is not IInteractable → fail");
                lootSub?.Dispose();
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            bool oreEnded = false;
            IDisposable endSub = ore.OnMiningEnd.Subscribe(_ => { oreEnded = true; });

            bool prevHold = interactor.IsHoldInteracting;
            interactor.IsHoldInteracting = true;

            // Стартуем удержание на самой руде, чтобы потом уметь его отменить вручную
            Coroutine holdRoutine = ore.StartCoroutine(oreInteractable.HoldInteract(interactor));

            // Ждём целевое количество либо исчерпание жилы/уничтожение объекта
            while (mined < desired && !oreEnded && ore != null && ore.isActiveAndEnabled)
                yield return null;

            // === Явная отмена удержания на руде ===
            if (ore != null) // могло уже удалиться при исчерпании
            {
                try
                {
                    oreInteractable.HoldInteractionCancel(interactor);
                }
                catch (Exception e)
                {
                    Debug.Log($"[ProgNodeMineOre] HoldInteractionCancel threw: {e.Message}");
                }
            }

            // Сбрасываем флаг удержания у бота
            interactor.IsHoldInteracting = prevHold;

            // Дадим один кадр корутине руды, чтобы она корректно завершилась
            yield return null;

            lootSub?.Dispose();
            endSub?.Dispose();

            if (mined <= 0)
            {
                Debug.Log("[ProgNodeMineOre] Mined 0 → fail");
                if (failureSlot?.ConnectedNode != null)
                    yield return executor.ExecuteNode(failureSlot.ConnectedNode);
                yield break;
            }

            _oreOutput.Value = ore;

            if (successSlot?.ConnectedNode != null)
                yield return executor.ExecuteNode(successSlot.ConnectedNode);
        }

        private static int ParseDesiredAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 1;

            text = text.Trim();

            if (int.TryParse(text, out int asInt))
                return asInt < 1 ? 1 : asInt;

            if (float.TryParse(text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out float asFloat))
            {
                int v = (int)asFloat;
                return v < 1 ? 1 : v;
            }

            return 1;
        }
    }
}
