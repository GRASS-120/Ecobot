using System;
using System.Collections.Generic;

namespace Handbook.Parser
{
    public class LineCursor
    {
        public int Index => _index;
        public bool End => _index >= _lines.Count;
        public string Current => End ? null : _lines[_index];

        private readonly List<string> _lines;
        private int _index;

        public LineCursor(List<string> lines)
        {
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            _index = 0;
        }

        public string Peek(int offset = 0)
        {
            var i = _index + offset;
            if (i < 0 || i >= _lines.Count)
                return null;
            return _lines[i];
        }

        public void Advance(int count = 1)
        {
            _index = Math.Min(_index + count, _lines.Count);
        }

        public Position Save()
        {
            return new Position { Index = _index };
        }

        public void Restore(Position pos)
        {
            _index = Math.Max(0, Math.Min(pos.Index, _lines.Count));
        }

        public struct Position
        {
            public int Index { get; set; }
        }
    }
}