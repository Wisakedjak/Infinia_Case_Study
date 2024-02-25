using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ClashRoyaleClone.Pool
{
    public class ProjectileSpawner
    {
        private GameObject prefab;
        private Transform parent;
        private List<Projectile> summonedProjectiles = new List<Projectile>();
        private List<Projectile> usedProjectiles = new List<Projectile>();
        
        public ProjectileSpawner(int count,GameObject projectilePrefab)
        {
            prefab = projectilePrefab;
            parent=new GameObject("ParticleParent").transform;
            for (int i = 0; i < count; i++)
            {
                SummonProjectile(parent);
            }
        }
        

        private Projectile GetProjectile()
        {
            if (summonedProjectiles.Count>0)
            {
                return summonedProjectiles[0];
            }
            else
            {
                SummonProjectile(parent);
                return summonedProjectiles[0];
            }
        }

        private void SummonProjectile(Transform parent)
        {
            var projectile = GameObject.Instantiate(prefab,parent).GetComponent<Projectile>();
            summonedProjectiles.Add(projectile);
        }
        
        private void ReturnProjectile(Projectile projectile)
        {
            summonedProjectiles.Add(projectile);
            usedProjectiles.Remove(projectile);
            projectile.gameObject.transform.parent = parent;
            projectile.transform.position = Vector3.zero;
        }
        
        
        public void Destroy()
        {
            foreach (var projectile in summonedProjectiles)
            {
                Object.Destroy(projectile.gameObject);
            }
            summonedProjectiles.Clear();
            foreach (var projectile in usedProjectiles)
            {
                Object.Destroy(projectile.gameObject);
            }
            usedProjectiles.Clear();
        }
    }
}