using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public Renderer arrow;
    public Vector3 target;
    public Text text;

    public void Set(Vector3 target, string text, Material mat)
    {
        arrow.sharedMaterial = mat;
        arrow.transform.LookAt(target);
        this.text.text = text;
    }

    public void Set(string text)
    {
        this.text.text = text;
    }

    public void Set(Vector3 target, string text)
    {
        arrow.transform.LookAt(target);
        this.text.text = text;
    }
}
