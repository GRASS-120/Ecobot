using System.Collections.Generic;

namespace Bot.Programming.Nodes.Base
{
    public class ProgNodeExecutionContext
    {
        public BotBase Bot { get; set; }
        
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
    
        public void SetVariable(string name, object value)
        {
            _variables[name] = value;
        }
    
        public T GetVariable<T>(string name, T defaultValue = default)
        {
            if (_variables.TryGetValue(name, out object value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
    }
}