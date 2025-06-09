using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Emoji : MonoBehaviour
{
    public float lifeTime = 8.0f;
    public float popDuration = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PopOn());
        StartCoroutine(WaitToDestroy());
    }
    void Update()
    {
        transform.rotation = Quaternion.Euler(45.0f, 0.0f, 0.0f);
        transform.localScale = new Vector3(1.0f / transform.parent.lossyScale.x, 1.0f / transform.parent.lossyScale.y, 1.0f / transform.parent.lossyScale.z);
    }

    IEnumerator PopOn()
    {
        float duration = popDuration; // Duration for scaling up
        float elapsedTime = 0f;

        // Start scale at 0
        transform.localScale = Vector3.zero;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches full scale
        transform.localScale = Vector3.one;
    }

    IEnumerator PopOff()
    {
        float duration = popDuration; // Duration for scaling down
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it reaches zero scale and then destroy
        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    IEnumerator WaitToDestroy()
    {
        yield return new WaitForSeconds(lifeTime);
        yield return PopOff(); // Wait for PopOff to complete before destroying
    }
}
