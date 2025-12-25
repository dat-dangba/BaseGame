using System.Collections.Generic;
using UnityEngine;

namespace SoundSample
{
    [CreateAssetMenu(fileName = "New SoundData", menuName = "ScriptableObject/New SoundData", order = 1)]
    public class SoundData : ScriptableObject
    {
        [SerializeField] private List<SoundItem> soundItems;

        public List<SoundItem> SoundItems => soundItems;
    }
}