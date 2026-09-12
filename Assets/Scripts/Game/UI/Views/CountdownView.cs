using Core.UI.Abstracts;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class CountdownView : BaseView
    {
        [SerializeField] private Text countdownText;
        [SerializeField] private Text hintText;

        public void SetCount(int remaining) => countdownText.text = remaining > 0 ? remaining.ToString() : GoLabel;

        public void SetHint(string hint) => hintText.text = hint;
    }
}
