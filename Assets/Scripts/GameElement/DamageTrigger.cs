using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float damage = 0.1f;
    [SerializeField] private float knockBack = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {

        IHitReactor reactor = collision.gameObject.GetComponent<IHitReactor>();
        if (reactor != null)
        {
            Vector3 knockBackDirection = collision.transform.position - transform.position;
            knockBackDirection.Normalize();

            IHitReactor.HitResult hitResult = reactor.Hit(IHitReactor.HitType.Bullet, damage, knockBackDirection * knockBack);
        }
    }
}
