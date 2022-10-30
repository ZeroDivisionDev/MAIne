using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    private void OnEnable()
    {
        StartCoroutine(MoveCloud());
    }

    IEnumerator MoveCloud()
    {
        while (true)
        {
            if(PlayerController.instance != null)
                transform.position = new Vector3(PlayerController.instance.transform.position.x, 120.5f, PlayerController.instance.transform.position.z);
            yield return new WaitForSeconds(2f);
        }
    }
}
