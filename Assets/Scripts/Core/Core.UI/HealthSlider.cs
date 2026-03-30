using Sirenix.OdinInspector;
using SteelSurge.Core.Network.HealthSystem.Components;
using SteelSurge.UI;
using UniRx;
using UnityEngine;

namespace SteelSurge.Core.UI
{
    [RequireComponent(typeof(ObservableFillSlider))]
    public class HealthSlider : MonoBehaviour
    {
       [SerializeField, ReadOnly] private ObservableFillSlider _slider;
        private HealthComponent _healthComponent;
        private CompositeDisposable _disposables;

        public void Initialize(HealthComponent healthComponent)
        {
            _disposables?.Clear();
            _healthComponent = healthComponent;
            _disposables = new();

            _healthComponent
                .HealthProperty
                .Subscribe(health =>
                {
                    _slider.MaxValue = health.MaxValue;
                    _slider.SetValueSmoothly(health.GetCurrentHealth());
                })
                .AddTo(_disposables);

                _healthComponent
                .OnDied
                .Subscribe(health =>
                {
                    _disposables?.Clear();
                    _healthComponent = null;
                    gameObject.SetActive(false);
                })
                .AddTo(_disposables);

            _slider.Value = healthComponent.CurrentHealth;
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        private void OnValidate()
        {
            if (!_slider)
            {
                _slider = GetComponent<ObservableFillSlider>();
            }
        }
    }
}
