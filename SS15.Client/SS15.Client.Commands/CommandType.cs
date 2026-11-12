namespace SS15.Client.Commands
{
    public enum CommandType : byte
    {
        Unknown = 0,
        PlaceTile = 1,
        RemoveTile = 2,
        ToggleDoor = 3,
        PickupItem = 4,
        DropItem = 5,
        ModifyAtmosphere = 6,
    }
}