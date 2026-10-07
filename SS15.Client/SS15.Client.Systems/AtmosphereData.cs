namespace SS15.Client.Systems
{
    public class AtmosphereData
    {
        // Температура в Кельвинах
        public float Temperature = 293.15f;

        // Количество вещества газов (условные единицы, не реальные моли)
        public float MolesOxygen = 21f;
        public float MolesNitrogen = 79f;
        public float MolesCarbonDioxide = 0.03f;

        // Суммарное количество вещества
        public float TotalMoles => MolesOxygen + MolesNitrogen + MolesCarbonDioxide;

        // Процентное содержание газов (вычисляется автоматически)
        public float OxygenPercent => TotalMoles > 0 ? MolesOxygen / TotalMoles * 100f : 0f;
        public float NitrogenPercent => TotalMoles > 0 ? MolesNitrogen / TotalMoles * 100f : 0f;
        public float CarbonDioxidePercent => TotalMoles > 0 ? MolesCarbonDioxide / TotalMoles * 100f : 0f;

        // Давление в кПа (пропорционально молям и температуре, объём = 1)
        public float Pressure => TotalMoles * 0.003455f * Temperature;
    }
}