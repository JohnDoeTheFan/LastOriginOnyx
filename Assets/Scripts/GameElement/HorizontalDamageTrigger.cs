using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalDamageTrigger : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float damage = 0.1f;
    [SerializeField] private float knockBack = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IHitReactor reactor = collision.gameObject.GetComponent<IHitReactor>();
        if (reactor != null)
        {
            Vector3 knockBackDirection = (transform.position.x < collision.transform.position.x) ? Vector3.right : Vector3.left;

            IHitReactor.HitResult hitResult = reactor.Hit(new IHitReactor.HitInfo(IHitReactor.HitType.Bullet, damage, knockBackDirection, false, knockBackDirection * knockBack));
        }
    }
}
