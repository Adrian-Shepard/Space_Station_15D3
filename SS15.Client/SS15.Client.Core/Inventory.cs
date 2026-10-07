using System;
using SS15.Client.Player;

namespace SS15.Client.Core
{
    public enum SlotType
    {
        Hand,
        Pocket,
        Belt,
        Back
    }

    public class Inventory
    {
        public Item[] Slots;

        public Inventory()
        {
            Slots = new Item[8];
        }

        public Item GetItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length) return null;
            return Slots[slotIndex];
        }

        public void SetItem(int slotIndex, Item item)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length) return;
            Slots[slotIndex] = item;
        }

        public Item RemoveItem(int slotIndex)
        {
            var item = GetItem(slotIndex);
            if (item != null) Slots[slotIndex] = null;
            return item;
        }

        public bool CanPlaceInSlot(int slotIndex, Item item)
        {
            return true;
        }

        public bool AddToSlot(int slotIndex, Item item)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Length) return false;
            if (Slots[slotIndex] != null) return false;
            if (!CanPlaceInSlot(slotIndex, item)) return false;
            Slots[slotIndex] = item;
            return true;
        }

        public bool AddToHand(Item item)
        {
            for (int i = 0; i <= 1; i++)
                if (Slots[i] == null) { Slots[i] = item; return true; }
            return false;
        }

        public bool AddToPocket(Item item)
        {
            for (int i = 2; i <= 5; i++)
                if (Slots[i] == null) { Slots[i] = item; return true; }
            return false;
        }

        public bool AddToBelt(Item item)
        {
            if (Slots[6] == null) { Slots[6] = item; return true; }
            return false;
        }

        public bool AddToBack(Item item)
        {
            if (Slots[7] == null) { Slots[7] = item; return true; }
            return false;
        }

        public SlotType GetSlotType(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex <= 1) return SlotType.Hand;
            if (slotIndex >= 2 && slotIndex <= 5) return SlotType.Pocket;
            if (slotIndex == 6) return SlotType.Belt;
            if (slotIndex == 7) return SlotType.Back;
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        }

        public bool UseItem(int slotIndex, PlayerController player)
        {
            var item = GetItem(slotIndex);
            if (item == null) return false;

            if (item is BagItem bag)
            {
                return true;
            }

            switch (item.Type)
            {
                case ItemType.Medkit:
                    player.Heal(30);
                    item.Quantity--;
                    if (item.Quantity <= 0) RemoveItem(slotIndex);
                    return true;
                case ItemType.Food:
                    player.Heal(10);
                    item.Quantity--;
                    if (item.Quantity <= 0) RemoveItem(slotIndex);
                    return true;
                case ItemType.Tool:
                    return true;
            }
            return false;
        }
    }
}