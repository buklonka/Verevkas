using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarParticleEffect : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] starSprites;

    [Header("Shape Settings")]
    [SerializeField] private float sphereRadius = 5f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 90f;

    void Start()
    {
        ConfigureParticleSystem();
        GetComponent<ParticleSystem>().Play(); // Используем встроенное свойство particleSystem
    }

    private void ConfigureParticleSystem()
    {
        ConfigureTextures();
        ConfigureMainModule();
        ConfigureEmission();
        ConfigureShape();
        ConfigureForces();
        ConfigureColorOverLifetime();
        ConfigureSizeOverLifetime();
        ConfigureRotationOverLifetime();
    }

    private void ConfigureTextures()
    {
        if (starSprites == null || starSprites.Length == 0) return;

        var textureSheetAnimation = GetComponent<ParticleSystem>().textureSheetAnimation;
        textureSheetAnimation.enabled = true;

        foreach (var sprite in starSprites)
        {
            textureSheetAnimation.AddSprite(sprite);
        }

        textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
        textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0, starSprites.Length - 1);
    }

    private void ConfigureMainModule()
    {
        var mainModule = GetComponent<ParticleSystem>().main;
        mainModule.startSpeed = 5f;
        mainModule.startLifetime = 2f;
        mainModule.startSize = 0.5f;
        mainModule.maxParticles = 100;
    }

    private void ConfigureEmission()
    {
        var emissionModule = GetComponent<ParticleSystem>().emission;
        emissionModule.rateOverTime = 20;
    }

    private void ConfigureShape()
    {
        var shapeModule = GetComponent<ParticleSystem>().shape;
        shapeModule.shapeType = ParticleSystemShapeType.Sphere;
        shapeModule.radius = sphereRadius;
    }

    private void ConfigureForces()
    {
        var forceOverLifetime = GetComponent<ParticleSystem>().forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f);
    }

    private void ConfigureColorOverLifetime()
    {
        var colorOverLifetime = GetComponent<ParticleSystem>().colorOverLifetime;
        colorOverLifetime.enabled = true;

        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );

        colorOverLifetime.color = gradient;
    }

    private void ConfigureSizeOverLifetime()
    {
        var sizeOverLifetime = GetComponent<ParticleSystem>().sizeOverLifetime;
        sizeOverLifetime.enabled = true;

        var sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f)
        );

        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }

    private void ConfigureRotationOverLifetime()
    {
        var rotationOverLifetime = GetComponent<ParticleSystem>().rotationOverLifetime;
        rotationOverLifetime.enabled = true;

        var mainModule = GetComponent<ParticleSystem>().main;
        var rotationCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(1f, rotationSpeed * mainModule.startLifetime.constant)
        );

        rotationOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, rotationCurve);
    }
}