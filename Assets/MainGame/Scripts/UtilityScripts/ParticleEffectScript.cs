using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ParticleEffectScript : MonoBehaviour
{
    [SerializeField] private float destroyTime;

    [SerializeField] private bool IncreaseScale;
    [SerializeField] private float IncreaseScaleValue;
    [SerializeField] private float IncreaseScaleTime;

    [SerializeField] private bool IncreaseFade;
    [SerializeField] private float IncreaseFadeDelay;
    [SerializeField] private float IncreaseFadeEndValue;
    [SerializeField] private float IncreaseFadeTime;

    [SerializeField] private bool Rotate;
    [SerializeField] private float rotateValue;
    [SerializeField] private float rotateDuration;

    [SerializeField] private bool canChangeColour;
    [SerializeField] private Color color;
    [SerializeField] private float colorChangeDelay;
    [SerializeField] private float colorChangeDuration;

    [SerializeField] private bool shake;
    [SerializeField] private float shakeStrength;

    [SerializeField] private bool shakeScale;
    [SerializeField] private float shakeScaleStrength;

    private void Start()
    {
        ChangeScale();
        StartCoroutine(ChangeFade());
        ChangeRotation();
        StartCoroutine(ChangeColour());
        Shake();
        ShakeScale();
        Destroy(gameObject, destroyTime);
    }

    private void ChangeScale()
    {
        if (IncreaseScale == false)
            return;

        transform.DOScale(IncreaseScaleValue, IncreaseScaleTime);
    }
    private IEnumerator ChangeFade()
    {
        if (IncreaseFade == true)
        {
            yield return new WaitForSeconds(IncreaseFadeDelay);
            GetComponent<SpriteRenderer>().DOFade(IncreaseFadeEndValue, IncreaseFadeTime);
        }
    }

    private void ChangeRotation()
    {
        if (Rotate == false)
            return;

        transform.DORotate(new Vector3(0,0, rotateValue), rotateDuration).SetLoops(100);
    }
    private IEnumerator ChangeColour()
    {
        if (canChangeColour == true)
        {
            yield return new WaitForSeconds(colorChangeDelay);
            GetComponent<SpriteRenderer>().DOColor(color, colorChangeDuration);
        }
    }

    private void Shake()
    {
        if (shake == true)
            transform.GetChild(0).DOShakePosition(destroyTime,shakeStrength);
    }
    private void ShakeScale()
    {
        if (shakeScale == true)
            transform.DOShakeScale(2f).SetLoops(-1, LoopType.Restart);
    }
}
