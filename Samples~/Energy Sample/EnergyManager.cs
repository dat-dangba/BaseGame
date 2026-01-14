using System;
using TMPro;
using UnityEngine;

namespace DBD.BaseGame.Sample
{
    public class EnergyManager : BaseEnergyManager<EnergyManager>
    {
        public TextMeshProUGUI energy;
        public TextMeshProUGUI nextRegenTime;
        public TextMeshProUGUI unlimitedRemainingTime;

        protected override DateTime GetDateTime()
        {
            return DateTime.UtcNow;
        }

        protected override void UpdateEnergyData(EnergyData energyData)
        {
            Save(energyData);
        }

        protected override void Start()
        {
            base.Start();
            EnergyData data = Load();
            // data NeedConsumeEnergy khi chơi game thì chuyển sang true, khi kết thúc game chuyển sang false
            // Khi đang chơi mà thoát game thì NeedConsumeEnergy = true, vào game lại sẽ check và trừ Energy 
            Init(data);
        }

        protected override void Update()
        {
            base.Update();
            energy.text = $"{Energy} \t {CanPlay()}";
            nextRegenTime.text = GetNextRegenTime();
            unlimitedRemainingTime.text = GetUnlimitedRemainingTime();
        }

        #region Save / Load

        private const string SAVE_KEY = "ENERGY_DATA";

        private void Save(EnergyData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private EnergyData Load()
        {
            EnergyData data = new EnergyData();
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                data.Energy = 5;
                data.LastRegenTime = ""; //GetDateTime().ToString("o");
                data.UnlimitedEndTime = ""; //GetDateTime().ToString("o");
                data.NeedConsumeEnergy = false;
                return data;
            }

            string json = PlayerPrefs.GetString(SAVE_KEY);
            data = JsonUtility.FromJson<EnergyData>(json);
            return data;
        }

        #endregion
    }
}