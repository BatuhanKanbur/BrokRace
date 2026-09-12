using System;
using System.Collections.Generic;
using System.Text;
using Core.UI.Abstracts;
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

        private readonly StringBuilder _builder = new();

        public event Action OnRestartClicked;

        protected override void Initialize()
        {
            base.Initialize();
            restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
        }

        public void ShowResults(IReadOnlyList<ICarProgress> order, ICarProgress player)
        {
            headlineText.text = player.FinishOrder == 1
                ? WinnerLabel
                : string.Format(FinishedFormat, player.FinishOrder, order.Count);
            _builder.Clear();
            foreach (var car in order)
                _builder.AppendLine(string.Format(ResultRowFormat, car.FinishOrder, car.DisplayName,
                    car.FinishTime, car.FinishTime - player.FinishTime));
            tableText.text = _builder.ToString();
        }
    }
}
