using System;
using Core.UI.Abstracts;
using Game.Ai.Enums;
using Game.Ai.Logics;
using UnityEngine;
using UnityEngine.UI;
using static Game.UI.Constants.HudConstants;

namespace Game.UI.Views
{
    public class SetupView : BaseView
    {
        [SerializeField] private Button[] rivalButtons;
        [SerializeField] private Button[] balanceButtons;
        [SerializeField] private Button startButton;

        public event Action<RivalMode> OnRivalSelected;
        public event Action<bool> OnBalanceSelected;
        public event Action OnStartClicked;

        protected override void Initialize()
        {
            base.Initialize();
            for (var index = 0; index < rivalButtons.Length; index++)
            {
                var mode = (RivalMode)index;
                rivalButtons[index].onClick.AddListener(() => OnRivalSelected?.Invoke(mode));
            }
            for (var index = 0; index < balanceButtons.Length; index++)
            {
                var enabled = index == 0;
                balanceButtons[index].onClick.AddListener(() => OnBalanceSelected?.Invoke(enabled));
            }
            startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
        }

        public void Refresh(RivalMode rivals, bool balancing)
        {
            for (var index = 0; index < rivalButtons.Length; index++)
                rivalButtons[index].image.color = (RivalMode)index == rivals ? SelectedColor : BlockedColor;
            for (var index = 0; index < balanceButtons.Length; index++)
                balanceButtons[index].image.color = (index == 0) == balancing ? SelectedColor : BlockedColor;
        }
    }
}
