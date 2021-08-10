using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuckShotBullet : Bullet
{
    [SerializeField]
    private Bullet ball;
    [SerializeField]
    private int numBall = 5;
    [SerializeField]
    private float ballSpread = 20;
    [SerializeField]
    private float ballSpeedAddictionRandomRange = 20;
    [SerializeField]
    private float speedMultiplier = 0.8f;
    [SerializeField]
    private float power;

    private readonly UnsubscriberPack ballUnsubscriberPack = new UnsubscriberPack();

    private void OnDestroy()
    {
        ballUnsubscriberPack.UnsubscribeAll();
    }

    public override void Propel(float power)
    {
        startPosition = transform.position;
        startTime = Time.time;

        this.power = power;
        rigidBody.gravityScale = 0;

        rigidBody.AddForce(transform.rotation * new Vector3(0, power * speedMultiplier, 0) * rigidBody.mass, ForceMode2D.Impulse);

        StartCoroutine(Job(() => WaitForSecondsRoutine(0.05f), () => Divide()));

    }

    public void Divide()
    {
        rigidBody.AddTorque(UnityEngine.Random.Range(-1000, 1000));
        rigidBody.gravityScale = 1;

        List<Bullet> newBalls = new List<Bullet>();
        for(int i = 0; i < numBall; i++)
        {
            newBalls.Add(Instantiate<Bullet>(ball, transform.position, transform.rotation * Quaternion.Euler(0, 0, UnityEngine.Random.Range(-ballSpread, ballSpread))));
        }

        foreach (var newBall in newBalls)
        {
            ballUnsubscriberPack.Add(new BallSubscriber(newBall, this));

            collisionException.Add(newBall.gameObject);
            newBall.shootSourceId = shootSourceId;

            newBall.Propel(power + UnityEngine.Random.Range(-ballSpeedAddictionRandomRange, ballSpeedAddictionRandomRange));
        }
    }

    public class BallSubscriber : UniUnsubscriber, Bullet.ISubscriber
    {
        readonly BuckShotBullet buckShotBullet;

        public BallSubscriber(Bullet ball, BuckShotBullet buckShotBullet)
        {
            InitUniSubscriber(ball.SubscribeManager.Subscribe(this));
            this.buckShotBullet = buckShotBullet;
        }

        void ISubscriber.OnHit(Bullet bullet, Collider2D collider)
        {
            buckShotBullet.SubscribeManager.ForEach(item => item.OnHit(buckShotBullet, collider));
        }

        void ISubscriber.OnHit(Bullet bullet, Collision2D collision)
        {
            buckShotBullet.SubscribeManager.ForEach(item => item.OnHit(buckShotBullet, collision));
        }

        void ISubscriber.OnHit(Bullet bullet, IHitReactor hitReactor, IHitReactor.HitResult hitResult)
        {
            buckShotBullet.SubscribeManager.ForEach(item => item.OnHit(buckShotBullet, hitReactor, hitResult));
        }

        void ISubscriber.OnExplode(Bullet bullet, Explosion explosion)
        {
            buckShotBullet.SubscribeManager.ForEach(item => item.OnExplode(buckShotBullet, explosion));
        }

        void ISubscriber.BeforeDestroy(Bullet bullet)
        {
            Unsubscribe();
        }

    }
}
