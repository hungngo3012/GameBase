using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GarageMesh : MonoBehaviour
{
    [SerializeField] List<MeshRenderer> changeColorRenderers;
    [SerializeField] List<int> changeColorMatIndexs;
    public TextMeshProUGUI countTxt;
    internal void ChangeColor(Color color)
    {
        int i = 0;
        foreach(MeshRenderer meshRenderer in changeColorRenderers)
        {
            meshRenderer.materials[changeColorMatIndexs[i]].color = color;
            i++;
        }    
    }
}
