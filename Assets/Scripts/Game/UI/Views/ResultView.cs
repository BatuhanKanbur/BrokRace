using System;
using System.Collections.Generic;
using System.Text;
using Core.UI.Abstracts;
using Core.Utilities;
using Cysharp.Threading.Tasks;
using Game.Cars.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class ResultView : BaseView
    {
        [SerializeField] private Text headlineText;
        [SerializeField] private Text tableText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button modesButton;

        private readonly StringBuilder _builder = new();

        public event Action OnRestartClicked;
        public event Action OnModesClicked;

        protected override void Initialize()
        {
            base.Initialize();
            restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            modesButton.onClick.AddListener(() => OnModesClicked?.Invoke());
        }

        public void ShowResults(IReadOnlyList<ICarProgress> order, ICarProgress player)
        {
            var won = player.FinishOrder == 1;
            headlineText.text = won ? WinnerLabel : string.Format(FinishedFormat, player.FinishOrder, order.Count);
            headlineText.color = won ? WinnerColor : PlacedColor;
            headlineText.rectTransform.GoPunch(HeadlinePunchScale, HeadlinePunchDuration,
                this.GetCancellationTokenOnDestroy());
            _builder.Clear();
            foreach (var car in order)
                _builder.AppendLine(string.Format(ResultRowFormat, car.FinishOrder, car.DisplayName,
                    car.FinishTime, car.FinishTime - player.FinishTime));
            tableText.text = _builder.ToString();
        }
    }
}
