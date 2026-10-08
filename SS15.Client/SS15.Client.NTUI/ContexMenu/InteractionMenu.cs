using System;
using System.Collections.Generic;

namespace SS15.Client.NTUI
{
    public class InteractionMenuItem
    {
        public string Label;
        public Action Execute;

        public InteractionMenuItem(string label, Action execute)
        {
            Label = label;
            Execute = execute;
        }
    }

    /// <summary>
    /// Список действий для тайла под прицелом. Заполняется в MainGame в момент открытия.
    /// </summary>
    public class InteractionMenu
    {
        public string Title { get; set; } = "";
        public List<InteractionMenuItem> Items { get; } = new();
        public int HoveredIndex { get; set; } = -1;

        public bool IsOpen => Items.Count > 0;

        public void Clear()
        {
            Items.Clear();
            Title = "";
            HoveredIndex = -1;
        }

        public void Add(string label, Action execute)
        {
            Items.Add(new InteractionMenuItem(label, execute));
        }

        public InteractionMenuItem GetHovered()
        {
            if (HoveredIndex < 0 || HoveredIndex >= Items.Count) return null;
            return Items[HoveredIndex];
        }
    }
}