using System;
using System.Collections.Generic;
using System.Text;
using System;
namespace MultiplayerPlugin
{
    public enum ColliderType { Sphere, Box }
    public abstract class Collider
    {
        
        public Vector3 center;
        public Action<Collider> onTriggerEnter, onTriggerExit;
        private List<Collider> collidedWith, prevCollidedWith;
        public Collider()
        {
            prevCollidedWith = new List<Collider>();
            collidedWith = new List<Collider>();            
        }
        public void OnCollision(Collider other)
        {
            collidedWith.Add(other);
        }
        public void Update(Vector3 center)
        {
            this.center = center;
            foreach(var collider in collidedWith)
            {                
                if(!prevCollidedWith.Contains(collider))
                {
                    onTriggerEnter?.Invoke(collider);
                }
            }
            foreach(var collider in prevCollidedWith)
            {                
                if (!collidedWith.Contains(collider))
                {
                    onTriggerExit?.Invoke(collider);
                }
            }

            prevCollidedWith.Clear();
            prevCollidedWith.AddRange(collidedWith);
            collidedWith.Clear();
        }
        public abstract ColliderType ColliderType { get; }
        public abstract bool CheckCollision_WithSphere(SphereCollider other);        
    }
    public class SphereCollider : Collider
    {
        public float radius;
        public override ColliderType ColliderType => ColliderType.Sphere;

        public override bool CheckCollision_WithSphere(SphereCollider other)
        {
            float distanceSquared = (center - other.center).sqrMagnitude;                      
            return distanceSquared <= (radius + other.radius) * (radius + other.radius);
        }
        public SphereCollider(float radius) : base()
        {
            this.radius = radius;
        }
    }

}
