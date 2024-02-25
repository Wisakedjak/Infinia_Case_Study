using System;
using System.Collections.Generic;
using ClashRoyaleClone.Spawns;
using UnityEngine;

namespace ClashRoyaleClone
{
    public class Spell : MonoBehaviour
    {
        private void Start()
        {
            Invoke("DamageTargets", .5f);
        }

        public List<Humanoid> _targets=new List<Humanoid>();
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Humanoid>())
            {
                Debug.Log("add target");
                _targets.Add(other.GetComponent<Humanoid>());
            }
        }
        
        private void DamageTargets()
        {
            foreach (var target in _targets)
            {
                if (target!=null)
                {
                    Debug.Log("damage target");
                    target.SufferDamage(10);
                }
                
            }
        }
    }
}