using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleClone.UI
{
    public class HpBar : MonoBehaviour
    {
        public Slider hpBarSlider;
        public Slider easeHpBarSlider;
        public float maxHp;
        public float currentHp;
        private float _lerpSpeed = 0.05f;

        private void Start()
        {
            
        }
        
        public void SetHpBar(float hp)
        {
            maxHp = hp;
            currentHp = hp;
            hpBarSlider.maxValue = maxHp;
            hpBarSlider.value = currentHp;
            easeHpBarSlider.maxValue = maxHp;
            easeHpBarSlider.value = currentHp;
        }

        private void Update()
        {
            
            if (hpBarSlider.value!= currentHp) 
            {
                hpBarSlider.value = currentHp;
            }
            if (easeHpBarSlider.value != hpBarSlider.value)
            {
                easeHpBarSlider.value = Mathf.Lerp(easeHpBarSlider.value, currentHp, _lerpSpeed);
            }
        }
        
        public void TakeDamage(float damage)
        {
            currentHp -= damage;
            if (currentHp < 0)
            {
                currentHp = 0;
            }
        }
    }
}