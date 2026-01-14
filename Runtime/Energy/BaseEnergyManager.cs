using System;
using System.Globalization;
using UnityEngine;

namespace DBD.BaseGame
{
    public abstract class BaseEnergyManager<INSTANCE> : BaseMonoBehaviour where INSTANCE : BaseMonoBehaviour
    {
        [SerializeField] private int maxEnergy = 5;
        [SerializeField] private int regenMinutes = 30;

        private bool isInit;

        private DateTime lastRegenTime;
        private DateTime unlimitedEndTime;

        [SerializeField] private EnergyData data = new();
        public int Energy => data.Energy;

        #region Singleton

        private static INSTANCE instance;

        public static INSTANCE Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindFirstObjectByType<INSTANCE>();
                if (instance != null) return instance;
                GameObject singleton = new(typeof(INSTANCE).Name);
                instance = singleton.AddComponent<INSTANCE>();
                DontDestroyOnLoad(singleton);

                return instance;
            }
        }

        protected override void Awake()
        {
            if (instance == null)
            {
                instance = this as INSTANCE;
                Transform root = transform.root;
                if (root != transform)
                {
                    DontDestroyOnLoad(root);
                }
                else
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        protected abstract DateTime GetDateTime();

        protected abstract void UpdateEnergyData(EnergyData energyData);

        public void Init(EnergyData energyData)
        {
            // data = new EnergyData
            // {
            //     Energy = GetEnergy(),
            //     LastRegenTime = GetLastRegenTime(),
            //     UnlimitedEndTime = GetUnlimitedEndTime(),
            // };
            data = energyData;
            ValidateTime();
            UpdateEnergy();

            isInit = true;
        }

        protected override void Update()
        {
            base.Update();
            UpdateEnergy();
        }

        #region Public API

        public bool IsUnlimitedEnergy => GetDateTime() < unlimitedEndTime;

        public bool CanPlay()
        {
            return IsUnlimitedEnergy || data.Energy > 0;
        }

        public void ConsumeEnergy()
        {
            if (!isInit || IsUnlimitedEnergy || data.Energy <= 0) return;

            data.Energy--;

            if (data.Energy == maxEnergy - 1)
            {
                lastRegenTime = GetDateTime();
                data.LastRegenTime = FormatDateTime(lastRegenTime);
            }

            UpdateEnergyData(data);
        }

        public void AddEnergy(int amount)
        {
            if (!isInit) return;

            data.Energy = Mathf.Min(
                data.Energy + amount,
                maxEnergy
            );

            UpdateEnergyData(data);
        }

        public void RemoveEnergy(int amount)
        {
            if (!isInit) return;

            data.Energy = Mathf.Max(
                data.Energy - amount,
                0
            );

            if (data.Energy == maxEnergy - 1)
            {
                lastRegenTime = GetDateTime();
                data.LastRegenTime = FormatDateTime(lastRegenTime);
            }

            UpdateEnergyData(data);
        }

        public void AddUnlimited(int seconds)
        {
            if (!isInit) return;

            var now = GetDateTime();

            if (unlimitedEndTime < now)
            {
                unlimitedEndTime = now;
            }

            unlimitedEndTime = unlimitedEndTime.AddSeconds(seconds);
            data.UnlimitedEndTime = FormatDateTime(unlimitedEndTime);

            UpdateEnergyData(data);
        }

        public string GetUnlimitedRemainingTime()
        {
            TimeSpan timeSpan = GetUnlimitedRemainingTimeSpan();
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        public TimeSpan GetUnlimitedRemainingTimeSpan()
        {
            if (!isInit || !IsUnlimitedEnergy)
                return TimeSpan.Zero;

            return unlimitedEndTime - GetDateTime();
        }

        public string GetNextRegenTime()
        {
            TimeSpan timeSpan = GetNextRegenTimeSpan();
            return $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }

        public TimeSpan GetNextRegenTimeSpan()
        {
            if (!isInit || data.Energy >= maxEnergy)
                return TimeSpan.Zero;

            var nextTime = lastRegenTime.AddMinutes(regenMinutes);

            return nextTime - GetDateTime();
        }

        #endregion

        #region Core Logic

        private void UpdateEnergy()
        {
            if (!isInit) return;

            if (data.NeedConsumeEnergy)
            {
                data.NeedConsumeEnergy = false;
                ConsumeEnergy();
            }
            // if (IsUnlimitedEnergy)
            //     return;

            if (data.Energy >= maxEnergy)
                return;

            var now = GetDateTime();
            var elapsedMinutes = (now - lastRegenTime).TotalMinutes;

            if (elapsedMinutes < regenMinutes)
                return;

            int regenCount = (int)(elapsedMinutes / regenMinutes);

            data.Energy = Mathf.Min(
                data.Energy + regenCount,
                maxEnergy
            );

            lastRegenTime = lastRegenTime.AddMinutes(regenCount * regenMinutes);
            data.LastRegenTime = FormatDateTime(lastRegenTime);

            UpdateEnergyData(data);
        }

        private void ValidateTime()
        {
            if (string.IsNullOrEmpty(data.LastRegenTime))
            {
                lastRegenTime = DateTime.UtcNow;
            }
            else
            {
                DateTimeOffset lastRegenDateTimeOffset =
                    DateTimeOffset.Parse(data.LastRegenTime, CultureInfo.InvariantCulture);
                lastRegenTime = lastRegenDateTimeOffset.UtcDateTime;
            }

            if (string.IsNullOrEmpty(data.UnlimitedEndTime))
            {
                unlimitedEndTime = DateTime.UtcNow;
            }
            else
            {
                DateTimeOffset unlimitedEndTimeOffset =
                    DateTimeOffset.Parse(data.UnlimitedEndTime, CultureInfo.InvariantCulture);
                unlimitedEndTime = unlimitedEndTimeOffset.UtcDateTime;
            }

            var now = GetDateTime();

            if (lastRegenTime > now)
            {
                lastRegenTime = now;
                data.LastRegenTime = lastRegenTime.ToString("o");
            }

            if (unlimitedEndTime > now.AddDays(1))
            {
                unlimitedEndTime = now;
                data.UnlimitedEndTime = FormatDateTime(unlimitedEndTime);
            }
        }

        #endregion

        private string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("o");
        }
    }
}