using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class StarParticleEffect : MonoBehaviour
{
    public Sprite[] starSprites; // Массив спрайтов звёздочек
    public float sphereRadius = 5f; // Радиус сферы
    public float rotationSpeed = 90f; // Скорость вращения частиц (градусов в секунду)

    void Start()
    {
        // Получаем компонент ParticleSystem
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        // Настраиваем текстуры частиц
        if (starSprites != null && starSprites.Length > 0)
        {
            // Создаём текстуру для частиц
            ParticleSystem.TextureSheetAnimationModule textureSheetAnimation = particleSystem.textureSheetAnimation;
            textureSheetAnimation.enabled = true;

            // Добавляем спрайты звёздочек в текстуру
            for (int i = 0; i < starSprites.Length; i++)
            {
                textureSheetAnimation.AddSprite(starSprites[i]);
            }

            // Настраиваем случайный выбор спрайтов
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;
            textureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(0, starSprites.Length - 1);
        }

        // Настраиваем параметры ParticleSystem
        ParticleSystem.MainModule mainModule = particleSystem.main;
        mainModule.startSpeed = 5f; // Скорость частиц
        mainModule.startLifetime = 2f; // Время жизни частиц
        mainModule.startSize = 0.5f; // Начальный размер частиц
        mainModule.maxParticles = 100; // Максимальное количество частиц

        // Настраиваем эмиссию (количество частиц в секунду)
        ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
        emissionModule.rateOverTime = 20;

        // Настраиваем форму эмиттера (сфера)
        ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
        shapeModule.shapeType = ParticleSystemShapeType.Sphere; // Форма сферы
        shapeModule.radius = sphereRadius; // Радиус сферы

        // Настраиваем гравитацию (чтобы звёздочки падали вниз)
        ParticleSystem.ForceOverLifetimeModule forceOverLifetime = particleSystem.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.y = new ParticleSystem.MinMaxCurve(-2f); // Гравитация вниз

        // Настраиваем цвет частиц
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;

        // Настраиваем уменьшение размера частиц
        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
        sizeOverLifetime.enabled = true;

        // Создаём кривую для плавного уменьшения размера
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f), // Начальный размер (100%)
            new Keyframe(1f, 0f)  // Конечный размер (0%)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Настраиваем вращение частиц по оси Y
        ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particleSystem.rotationOverLifetime;
        rotationOverLifetime.enabled = true;

        // Создаём кривую для плавного вращения по оси Y
        AnimationCurve rotationCurve = new AnimationCurve(
            new Keyframe(0f, 0f), // Начальный угол (0 градусов)
            new Keyframe(1f, rotationSpeed * mainModule.startLifetime.constant) // Конечный угол (градусы)
        );
        rotationOverLifetime.y = new ParticleSystem.MinMaxCurve(1f, rotationCurve);

        // Запускаем ParticleSystem
        particleSystem.Play();
    }
}