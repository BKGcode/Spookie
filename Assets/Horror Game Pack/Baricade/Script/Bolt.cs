using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    public Animator BoltAnimator;
    [SerializeField] private string UnScrew = "BoltAnim";
    public Rigidbody rigidbody;
    public float animLenght = 0.5f;
    public float DeleteLenght = 2f;
    public float FadeDuration = 2.0f;
    public bool WantToDisapear = true;
    [HideInInspector] public bool screwed = false;
    public Material ScrewMaterial;
    private Material fadeMaterialInstance;
    private bool isUnscrewed = false;
    [HideInInspector] public bool CanScrew = false;

    private void Start()
    {
        rigidbody.isKinematic = true;
    }
    public void unscrew()
    {
        if (!CanScrew) return;
        if (isUnscrewed) return;
        isUnscrewed = true;
        BoltAnimator.Play(UnScrew, 0, 0.0f);
        StartCoroutine(AfterScrew());
        fadeMaterialInstance = new Material(ScrewMaterial);
        GetComponent<Renderer>().material = fadeMaterialInstance;
    }
    IEnumerator AfterScrew()
    {
        yield return new WaitForSecondsRealtime(animLenght);
        BoltAnimator.enabled = false;
        rigidbody.isKinematic = false;
        screwed = true;
        if(WantToDisapear)
        {
            StartCoroutine(DeleteScrew());
        }
    }
    IEnumerator DeleteScrew()
    {
        yield return new WaitForSecondsRealtime(DeleteLenght);

        SetMaterialToFade(fadeMaterialInstance);

        float elapsedTime = 0f;
        Color originalColor = fadeMaterialInstance.color;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FadeDuration);
            fadeMaterialInstance.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        fadeMaterialInstance.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Destroy(this.gameObject);
    }
    private void SetMaterialToFade(Material material)
    {
        material.SetFloat("_Mode", 2);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}
