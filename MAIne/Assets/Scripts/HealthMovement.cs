using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthMovement : MonoBehaviour
{

    public Vector2 shake;
    public float WaveAmp;
    Vector3 initialPos;

    void Start()
    {
        Invoke("SetInitialPos", 0.1f);
    }

    void SetInitialPos()
    {
        initialPos = transform.position;
        //Debug.Log(initialPos);
    }

    public void HealthShake()
    {
        transform.position = initialPos;
        LeanTween.cancel(gameObject);
        HorizontalShake();
    }

    public void HealthWave()
    {
        transform.position = initialPos;
        LeanTween.cancel(gameObject);
        VerticalWave();
    }

    void VerticalWave()
    {
        LeanTween.moveY(gameObject,  WaveAmp + initialPos.y, 0.1f).setEaseInOutSine().setOnComplete(DefaultPosition);
    }

    void HorizontalShake()
    {
        LeanTween.moveX(gameObject, (Random.Range(0, 2) * 2 - 1) * shake.x + initialPos.x, 0.01f).setOnComplete(VerticalShake);
    }

    void VerticalShake()
    {
        LeanTween.moveY(gameObject, (Random.Range(0, 2) * 2 - 1) * shake.y + initialPos.y, 0.05f).setOnComplete(DefaultPosition);
    }

    void DefaultPosition()
    {
        LeanTween.move(gameObject, initialPos, 0.1f).setEaseInOutSine();
    }

}
