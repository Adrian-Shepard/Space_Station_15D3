namespace SS15.Client.Commands
{
    public struct GameCommandResult
    {
        public bool Success;
        public string Message;

        public static GameCommandResult Ok(string msg = null)
            => new GameCommandResult { Success = true, Message = msg };

        public static GameCommandResult Reject(string msg)
            => new GameCommandResult { Success = false, Message = msg };
    }
}