using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Onyx.Ability;
using Onyx.GameElement;

public class Jumpable : AbilityBase
{

    [SerializeField, Range(1, 20)]
    private float jumpVelocity = 3;
    [SerializeField]
    private GroundChecker groundChecker;
    [SerializeField]
    private Sprite skillImage;
    [SerializeField]
    private AudioSource jumpAudioSource;
    [SerializeField]
    private AudioSource landAudioSource;

    private Rigidbody2D rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        skills.Add(new ButtonActiveAbilitySKill("Jump", skillImage, 0.5f, new SkillDescription(), StartCoroutine, Jump));
    }

    public void Jump()
    {
        if (groundChecker.IsGrounded)
        {
            rigidBody.AddForce(Vector2.up * CalcImpulseForVelocity(jumpVelocity), ForceMode2D.Impulse);
            if(jumpAudioSource != null)
                jumpAudioSource.Play();
        }

        float CalcImpulseForVelocity(float velocity) => rigidBody.mass * velocity;
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
