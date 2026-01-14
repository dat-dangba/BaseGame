using Teo.AutoReference;
using TMPro;
using UnityEngine;

namespace DBD.BaseGame
{
    public abstract class BaseText : BaseMonoBehaviour
    {
        [SerializeField, Get] protected TextMeshProUGUI text;

        public virtual void SetText(string t)
        {
            text.text = t;
        }
    }
}