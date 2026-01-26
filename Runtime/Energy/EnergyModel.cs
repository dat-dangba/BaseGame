using System;

namespace DBD.BaseGame
{
    [Serializable]
    public class EnergyModel
    {
        public int Energy;
        public string LastRegenTime;
        public string UnlimitedEndTime;
        public bool NeedConsumeEnergy;
    }
}