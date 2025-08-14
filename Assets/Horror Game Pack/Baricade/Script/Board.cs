using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public Rigidbody BoardRigidbody;
    public List<Bolt> Bolts = new List<Bolt>();
    public bool WantToDisapear = true;
    public Material BaricadeMaterial;
    public float FadeDuration = 2.0f;
    private Material BaricadeMaterialInstance;

    private void Start()
    {
        BaricadeMaterialInstance = new Material(BaricadeMaterial);
        GetComponent<Renderer>().material = BaricadeMaterialInstance;
    }

    private void Update()
    {
        bool allBoltsScrewed = true;
        foreach (var bolt in Bolts)
        {
            if (!bolt.screwed)
            {
                allBoltsScrewed = false;
                break;
            }
        }

        if (allBoltsScrewed)
        {
            BoardRigidbody.isKinematic = false;
            if(WantToDisapear)
            {
                StartCoroutine(FadeAndDeleteBoard());
            }
        }
    }

    IEnumerator FadeAndDeleteBoard()
    {
        yield return new WaitForSecondsRealtime(5f);

        
        float elapsedTime = 0f;
        Color originalColor = BaricadeMaterialInstance.color;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeDuration);
            BaricadeMaterialInstance.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        BaricadeMaterialInstance.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Destroy(this.gameObject);
    }
}
