using System;

namespace DBD.BaseGame
{
    [Serializable]
    public class EnergyData
    {
        public int Energy;
        public string LastRegenTime;
        public string UnlimitedEndTime;
        public bool NeedConsumeEnergy;
    }
}