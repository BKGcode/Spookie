using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToBounce : MonoBehaviour
{

  public Material mat;
  public float dampingSpeed = 0.02f;
  float stretch;

  List<Material> mats;

  void Start()
  {
    mats = new List<Material>();
  }

  // Update is called once per frame update
  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      RaycastHit hit;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out hit))
      {
        Collider[] cols = Physics.OverlapSphere(hit.point, 10);
        ZeroOutMats();
        foreach (Collider col in cols)
        {
          Material mat = col.GetComponent<Renderer>().material;
          if (!mats.Contains(mat))
            mats.Add(mat);
          mat.SetVector("_point_of_bend", hit.point);
          if (col)
            mat.SetVector("_direction", col.transform.rotation * hit.transform.up);
          stretch = 4;
        }
      }
    }
    stretch = Mathf.Clamp(stretch - dampingSpeed, 0, 20);
    foreach (Material mat in mats)
    {
      mat.SetFloat("_stretch", stretch);
    }
  }

  void ZeroOutMats()
  {
    foreach (Material mat in mats)
    {
      mat.SetFloat("_stretch", 0);
    }
    mats.Clear();
  }
}
