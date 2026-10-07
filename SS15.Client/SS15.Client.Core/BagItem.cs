namespace SS15.Client.Core
{
    public class BagItem : Item
    {
        public Inventory Container { get; private set; }

        public BagItem(ItemType type, string name, int quantity, byte[] color, int capacity = 6)
            : base(type, name, quantity, color)
        {
            Container = new Inventory();
            System.Array.Resize(ref Container.Slots, capacity); // теперь работает, т.к. Slots - поле
        }
    }
}