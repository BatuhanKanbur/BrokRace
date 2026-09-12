using System;
using System.Collections.Generic;
using System.Text;
using Core.UI.Abstracts;
using Game.Cars.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Views
{
    public class ResultView : BaseView
    {
        [SerializeField] private Text headlineText;
        [SerializeField] private Text tableText;
        [SerializeField] private Button restartButton;

        public event Action OnRestartClicked;

        protected override void Initialize()
        {
            base.Initialize();
            restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
        }

        public void ShowResults(IReadOnlyList<ICarProgress> order, ICarProgress player)
        {
            headlineText.text = player.FinishOrder == 1 ? "WINNER" : $"FINISHED {player.FinishOrder}/{order.Count}";
            var table = new StringBuilder();
            foreach (var car in order)
                table.AppendLine($"{car.FinishOrder}.  {car.DisplayName,-12} {car.FinishTime:0.00}s");
            tableText.text = table.ToString();
        }
    }
}
