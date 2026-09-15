using Core.UI.Abstracts;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class CountdownView : BaseView
    {
        [SerializeField] private Text countdownText;
        [SerializeField] private Text hintText;

        public void SetCount(int remaining)
        {
            var launched = remaining <= 0;
            countdownText.text = launched ? GoLabel : remaining.ToString();
            countdownText.color = launched ? CountdownGoColor : CountdownCountColor;
            countdownText.rectTransform.GoPunch(launched ? GoPunchScale : CountPunchScale, CountPunchDuration,
                this.GetCancellationTokenOnDestroy());
        }

        public void SetHint(string hint) => hintText.text = hint;
    }
}
