using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerPlugin
{
    public class Physics
    {
        private static List<Collider> colliders;
        static Physics()
        {
            colliders = new List<Collider>();
        }
        public static void SubscribeForCollisionEvents(Collider collider)
        {
            colliders.Add(collider);
        }
        public static void CheckCollisions()
        {
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {                    
                    if (colliders[i].ColliderType == ColliderType.Sphere)
                    {
                        SphereCollider sphereCollider = colliders[i] as SphereCollider;
                        if (colliders[j].CheckCollision_WithSphere(sphereCollider))
                        {
                            colliders[i].OnCollision(colliders[j]);
                            colliders[j].OnCollision(colliders[i]);
                        }
                    }
                }
            }
        }
    }
}
