using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;
using Onyx.GameElement;

public class Jumpable : AbilityBase
{

    [SerializeField, Range(1, 20)]
    private float jumpVelocity = 3;
    [SerializeField, Range(1, 20)]
    private float dashVelocity = 10;
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

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        skills.Add(new ButtonActiveAbilitySKill("Jump", jumpSkillImage, 0.5f, new SkillDescription(), StartCoroutine, Jump));
        skills.Add(new ButtonActiveAbilitySKill("Dash", dashSkillImage, 1f, new SkillDescription(), StartCoroutine, Dash));
    }

    private void Jump()
    {
        if (groundChecker.IsGrounded)
        {
            rigidBody.AddForce(Vector2.up * jumpVelocity * rigidBody.mass, ForceMode2D.Impulse);
            if(jumpAudioSource != null)
                jumpAudioSource.Play();
        }
    }
    private void Dash()
    {
        Vector2 direction = abilityHolder.isFacingLeft ? Vector2.left : Vector2.right;
        rigidBody.AddForce(direction * dashVelocity * rigidBody.mass, ForceMode2D.Impulse);
        Instantiate<GameObject>(afterImagePrefab, transform.position, transform.rotation);

        StartCoroutine(AfterEffectCoroutine());
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
}
