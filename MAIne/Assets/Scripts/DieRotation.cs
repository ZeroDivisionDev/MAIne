using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotation : MonoBehaviour
{
    public Transform cam;
    public Transform armature;
    int rotateDirection;
    float rotationX;
    float rotationY;

    private void OnEnable()
    {
        rotateDirection = Random.Range(0, 2) * 2 - 1;
        rotationX = cam.rotation.eulerAngles.x;
        rotationY = cam.rotation.eulerAngles.y;
    }


    void FixedUpdate()
    {
        armature.rotation = Quaternion.Lerp(armature.rotation, Quaternion.Euler(0, rotationY, 90 * rotateDirection), 0.1f);
        cam.rotation = Quaternion.Lerp(cam.rotation, Quaternion.Euler(rotationX, rotationY, 20 * rotateDirection), 0.1f);
    }
}
