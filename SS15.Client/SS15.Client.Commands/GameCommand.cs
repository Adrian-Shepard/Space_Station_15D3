namespace SS15.Client.Commands
{
    public abstract class GameCommand
    {
        public int IssuerId { get; set; }
        public double Timestamp { get; set; }

        public abstract CommandType Type { get; }
        public abstract GameCommandResult Apply(GameContext ctx);
    }
}