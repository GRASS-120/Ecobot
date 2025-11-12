using System;
using Game;
using GUI.UIFramework;
using R3;
using UnityEngine; // для Debug

namespace GUI.Programming.Windows
{
    /// <summary>
    /// Контроллер окна оверлея программирования.
    /// Делает привязку Graph ← BotProgrammingController и отвечает за закрытие.
    /// </summary>
    public class ProgrammingOverlayController : WindowController<ProgrammingOverlayView>
    {
        private readonly GameManager _gameManager;
        public override string Id => "ProgrammingOverlay";

        // Флаг, чтобы не закрываться/не слать снапшот несколько раз подряд
        private bool _isClosing;

        public ProgrammingOverlayController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _isClosing = false;
            Subs.Clear(); // на всякий случай

            // Привязка графа к боту (делаем здесь, НЕ в BotBase)
            var holder = View.GetComponentInParent<Bot.ProgrammingOverlayBotHolder>(true);
            var botProg = holder ? holder.BotProgramming : null;
            if (View.Graph != null)
                View.Graph.SetBotProgramming(botProg);

            // Закрыть по кнопке "Close"
            if (View.BtnClose != null)
            {
                View.BtnClose.OnClickAsObservable()
                    .Subscribe(_ => RequestCloseOverlay())
                    .AddTo(Subs);
            }

            // ВАЖНО: закрытие по ESC УБРАНО полностью, чтобы не плодить альтернативный путь закрытия
            // и не оставлять незакрытые подписки/обработчики. Если нужно вернуть — делайте вызов
            // ровно того же метода RequestCloseOverlay() из единого места.
        }

        public override void OnClose()
        {
            // Сбрасываем флаг, чтобы при следующем открытии всё было чисто
            _isClosing = false;

            // Базовый OnClose() сам чистит Subs у WindowController, но подстрахуемся
            Subs.Clear();
            base.OnClose();
        }

        private void RequestCloseOverlay()
        {
            if (_isClosing) return;
            _isClosing = true;

            // Перед закрытием — отправить снапшот графа боту и сохранить JSON/SceneStorage
            try
            {
                View.Graph?.SendSnapshotToBot();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProgrammingOverlay] SendSnapshotToBot threw: {e}");
            }

            // Закрыть оверлей через ваш штатный путь (возврат в предыдущее состояние)
            try
            {
                _gameManager?.FSM?.GoToPreviousState();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ProgrammingOverlay] GoToPreviousState threw: {e}");
            }
        }
    }
}
