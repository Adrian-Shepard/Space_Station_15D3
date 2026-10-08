namespace SS15.Client.Systems
{
    public struct AtmosphereData
    {
        public float Temperature;
        public float MolesOxygen;
        public float MolesNitrogen;
        public float MolesCarbonDioxide;
        public float MolesPlasma;

        public float TotalMoles => MolesOxygen + MolesNitrogen + MolesCarbonDioxide + MolesPlasma;
        public float OxygenPercent => TotalMoles > 0 ? MolesOxygen / TotalMoles * 100f : 0f;
        public float NitrogenPercent => TotalMoles > 0 ? MolesNitrogen / TotalMoles * 100f : 0f;
        public float CarbonDioxidePercent => TotalMoles > 0 ? MolesCarbonDioxide / TotalMoles * 100f : 0f;
        public float PlasmaPercent => TotalMoles > 0 ? MolesPlasma / TotalMoles * 100f : 0f;
        public float Pressure => TotalMoles * 0.003455f * Temperature;
    }
}