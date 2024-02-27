using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class CursorMenu : IScrollable
    {
        private int _SelectedElement;
        public int SelectedElement
        {
            get
            {
                return _SelectedElement;
            }
            set
            {
                var delta = value - _SelectedElement;
                var forward = delta > 0;

                if (value >= Elements.Count)
                    _SelectedElement = 0;
                else if (value < 0)
                    _SelectedElement = Elements.Count - 1;
                else
                    _SelectedElement = value;

                var start = _SelectedElement;
                while (!(Elements[_SelectedElement] is CursorElement))
                {
                    _SelectedElement += forward ? 1 : -1;
                    if (_SelectedElement == start)
                        break;
                    if (_SelectedElement >= Elements.Count)
                        _SelectedElement = 0;
                    else if (_SelectedElement < 0)
                        _SelectedElement = Elements.Count - 1;
                }
            }
        }
        public List<ITextElement> Elements;

        public void Execute()
        {
            (this.Elements[this.SelectedElement] as CursorElement).Action();
        }

        public virtual string GetText(int availableWidth)
        {
            var text = "";
            var pos = 0;
            for (var i = 0; i < Elements.Count; i++)
            {
                if (Elements[i] is CursorElement)
                {
                    if (_SelectedElement == i) _ScrollPosStart = pos;
                    var line = $" {(_SelectedElement == i ? "<focus>►" : " ")} {Utils.WrapText(Elements[i].GetText(availableWidth - 3), availableWidth, "   ", "", false)}";
                    pos += line.Count<char>(c => c.Equals('\n'));
                    text += line;
                    if (_SelectedElement == i) _ScrollPosEnd = pos;
                }
                else
                {
                    var line = $"{Elements[i].GetText(availableWidth)}";
                    pos += line.Count<char>(c => c.Equals('\n'));
                    text += line;
                }
            }
            return text;
        }

        private int _ScrollPosStart = 0;
        private int _ScrollPosEnd = 0;
        private int _LastScrollPos = 0;
        public int GetScroll(int maxHeight)
        {
            var start = _LastScrollPos;
            var end = _LastScrollPos + maxHeight;
            var newScroll = _LastScrollPos;
            if (_ScrollPosEnd > end - 3)
                newScroll = _ScrollPosEnd + 3 - maxHeight;
            if (_ScrollPosStart < start + 3)
                newScroll = _ScrollPosStart - 3;
            if (newScroll < 0)
                newScroll = 0;
            _LastScrollPos = newScroll;
            return newScroll;
        }
    }
}