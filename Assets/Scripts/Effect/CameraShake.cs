using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Transform cam;
    private Vector3 originalPos;

    [Header("Shake SFX")]
    public AudioSource shakeSFX;  // 강한 카드 착지 사운드

    void Awake()
    {
        instance = this;
        cam = Camera.main.transform;
        originalPos = cam.localPosition;
    }

    // 외부에서 호출
    public static void Shake(float duration, float magnitude, bool playSound = true)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShakeCo(duration, magnitude, playSound));
        }
    }

    private IEnumerator ShakeCo(float duration, float magnitude, bool playSound)
    {
        float timer = 0f;

        // SFX 재생
        if (playSound && shakeSFX != null)
        {
            shakeSFX.Stop();
            shakeSFX.Play();
        }

        while (timer < duration)
        {
            float x = Random.Range(-0.5f, 0.5f) * magnitude;
            float y = Random.Range(-0.5f, 0.5f) * magnitude;

            cam.localPosition = originalPos + new Vector3(x, y, 0);

            timer += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalPos;
    }
}
