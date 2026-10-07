namespace SS15.Client.Core
{
    public enum ItemType
    {
        Medkit,
        Food,
        Tool
    }

    public class Item
    {
        public ItemType Type { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public byte[] Color { get; set; } // RGBA для отображения

        public Item(ItemType type, string name, int quantity, byte[] color)
        {
            Type = type;
            Name = name;
            Quantity = quantity;
            Color = color;
        }
    }
}