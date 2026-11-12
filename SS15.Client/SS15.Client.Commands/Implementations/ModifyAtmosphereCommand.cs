namespace SS15.Client.Commands
{
    public enum AtmosField
    {
        Oxygen,
        Nitrogen,
        CarbonDioxide,
        Temperature,
        Plasma,
    }

    public sealed class ModifyAtmosphereCommand : GameCommand
    {
        public int X;
        public int Z;
        public AtmosField Field;
        public float Delta;

        public override CommandType Type => CommandType.ModifyAtmosphere;

        public override GameCommandResult Apply(GameContext ctx)
        {
            var atmo = ctx.State.Atmosphere;
            switch (Field)
            {
                case AtmosField.Oxygen:       atmo.ModifyOxygen(X, Z, Delta); break;
                case AtmosField.Nitrogen:     atmo.ModifyNitrogen(X, Z, Delta); break;
                case AtmosField.CarbonDioxide:atmo.ModifyCarbonDioxide(X, Z, Delta); break;
                case AtmosField.Temperature:  atmo.ModifyTemperature(X, Z, Delta); break;
                case AtmosField.Plasma:       atmo.ModifyPlasma(X, Z, Delta); break;
            }
            return GameCommandResult.Ok();
        }
    }
}