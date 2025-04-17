namespace Utils
{
    /// <summary>
    /// Использовать в случае, когда нужно удобно переключаться между только двумя состояниями
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ToggleObject<T>
    {
        private readonly T[] _states;
        private int _currentIndex;
        
        public ToggleObject(T state1, T state2)
        {
            _states = new[] { state1, state2 };
            _currentIndex = 0;
        }

        public T GetState()
        {
            return _states[_currentIndex];
        }

        public void Toggle()
        { 
            _currentIndex = (_currentIndex + 1) % _states.Length;
        }
    }
}