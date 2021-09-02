using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;
using Onyx.GameElement;

public class Jumpable : AbilityBase, GroundChecker.ISubscriber
{

    [SerializeField, Range(1, 20)]
    private float jumpVelocity = 3;
    [SerializeField, Range(0, 5)]
    private int extraJumpNumber = 0;
    [SerializeField]
    private Vector2 dashVelocity;
    [SerializeField]
    private float dashRecoverTime = 0.2f;
    [SerializeField]
    private GroundChecker groundChecker;
    [SerializeField]
    private Sprite jumpSkillImage;
    [SerializeField]
    private Sprite dashSkillImage;
    [SerializeField]
    private AudioSource jumpAudioSource;
    [SerializeField]
    private AudioSource landAudioSource;
    [SerializeField]
    private GameObject afterImagePrefab;
    [SerializeField]
    private float afterImageTime = 0.2f;
    [SerializeField]
    private float afterImageTickTime = 0.05f;

    private Rigidbody2D rigidBody;
    private int extraJumpCount;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        skills.Add(new ButtonActiveAbilitySKill("Jump", jumpSkillImage, 0.1f, new SkillDescription(), StartCoroutine, Jump, ()=> !abilityHolder.isMovementOccupied));
        skills.Add(new ButtonActiveAbilitySKill("Dash", dashSkillImage, 1f, new SkillDescription(), StartCoroutine, Dash, () => !abilityHolder.isMovementOccupied));
        groundChecker.SubscribeManager.Subscribe(this);
    }

    private void Jump()
    {
        if(groundChecker.IsGrounded)
        {
            Jump();
        }
        else if(extraJumpCount < extraJumpNumber)
        {
            Jump();

            extraJumpCount++;
        }

        void Jump()
        {
            float jumpVelocityToAdd = jumpVelocity;
            if (rigidBody.velocity.y < 0)
                jumpVelocityToAdd += -rigidBody.velocity.y;
            else
                jumpVelocityToAdd -= rigidBody.velocity.y;

            rigidBody.AddForce(Vector2.up * jumpVelocityToAdd * rigidBody.mass, ForceMode2D.Impulse);
            if (jumpAudioSource != null)
                jumpAudioSource.Play();
        }
    }
    private void Dash()
    {
        abilityHolder.OccupyMovement(true);

        Vector2 dashVelocityToAdd = dashVelocity;

        if (abilityHolder.isFacingLeft)
            dashVelocityToAdd.x *= -1;

        if(dashVelocityToAdd.x * rigidBody.velocity.x > 0)
            dashVelocityToAdd.x -= rigidBody.velocity.x;

        abilityHolder.AddVelocity(dashVelocityToAdd, dashRecoverTime);

        Instantiate<GameObject>(afterImagePrefab, transform.position, transform.rotation);
        StartCoroutine(AfterEffectCoroutine());
        StartCoroutine(Job(()=>WaitForSecondsRoutine(0.2f), ()=>abilityHolder.OccupyMovement(false)));
    }

    private IEnumerator AfterEffectCoroutine()
    {
        float finishTime = Time.time + afterImageTime;
        while (Time.time < finishTime)
        {
            yield return new WaitForSeconds(afterImageTickTime);
            Instantiate<GameObject>(afterImagePrefab, transform.position, transform.rotation);
        }
    }

    public void PlayLandAudio()
    {
        if (landAudioSource != null)
            landAudioSource.Play();
    }

    public override void InstantiateAbilitySpecificGui(RectTransform abilitySpecificGuiArea)
    {
        return;
    }

    void GroundChecker.ISubscriber.OnGrounded()
    {
        extraJumpCount = 0;
    }

    void GroundChecker.ISubscriber.OnAir()
    {
    }
}
