using Core.UI.Abstracts;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Views
{
    public class CountdownView : BaseView
    {
        [SerializeField] private Text countdownText;
        [SerializeField] private Text hintText;

        public void SetCount(int remaining) => countdownText.text = remaining > 0 ? remaining.ToString() : "GO!";

        public void SetHint(string hint) => hintText.text = hint;
    }
}
