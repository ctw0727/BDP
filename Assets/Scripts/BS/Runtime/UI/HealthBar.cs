using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BS.UI
{
    public class HealthBar : MonoBehaviour
    {
        public Slider _slider;
        public Image _innerFillImage;

        public void Init()
        {
            _slider.gameObject.SetActive(true);
        }

        public void SetMaxHealth(float health)
        {
            _slider.maxValue = health;
            _slider.value = health;
            _innerFillImage.fillAmount = 1;
        }

        public void SetHealth(float health, float t = 1)
        {
            StartCoroutine(IncrementalSetHealth(health, t));
        }

        protected IEnumerator IncrementalSetHealth(float health, float t)
        {
            float duration = t;
            // delta가 음수라면 감소
            // delta가 양수라면 증가
            float delta = (health - _slider.value) / _slider.maxValue;
            float freq = delta / t;
            _slider.value = health;

            while (duration > 0)
            {
                _innerFillImage.fillAmount += (freq * Time.deltaTime);
                duration -= Time.deltaTime;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            _innerFillImage.fillAmount = health / _slider.maxValue;
            yield return null;
        }

        public void SetMaxHealth()
        {
            SetMaxHealth(1);
        }

        public void SetHealthTest()
        {
            SetHealth(_slider.value / 2);
        }
    }
}
