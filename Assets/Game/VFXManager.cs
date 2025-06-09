using NinthArt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class VFXManager : Singleton<VFXManager>
{
    public List<VFX> vfxs = new List<VFX>();
    public GameObject mouseClickVfx;
    internal static IEnumerator PlayVFX(Vfx type, Vector3 pos, Quaternion rot, float duration, Transform parent)
    {
        GameObject vfxObj = null;
        foreach(VFX vfx in Instance.vfxs)
        {
            if (vfx.vfxType == type)
            {
                vfxObj = Instantiate(vfx.vfxPrefab, pos, rot, parent);
                break;
            }
        }

        yield return new WaitForSeconds(duration);

        if(vfxObj != null)
            Destroy(vfxObj);
    }
    internal static IEnumerator PlayVFX(string vfxName, Vector3 pos, Quaternion rot, float duration, Transform parent)
    {
        GameObject vfxObj = null;
        foreach (VFX vfx in Instance.vfxs)
        {
            if (vfx.vfxPrefab.name == vfxName)
            {
                vfxObj = Instantiate(vfx.vfxPrefab, pos, rot, parent);
                break;
            }
        }

        yield return new WaitForSeconds(duration);

        if (vfxObj != null)
            Destroy(vfxObj);
    }
    internal static void PlayVFX(Vfx type, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject vfxObj = null;
        foreach (VFX vfx in Instance.vfxs)
        {
            if (vfx.vfxType == type)
            {
                vfxObj = Instantiate(vfx.vfxPrefab, pos, rot, parent);
                break;
            }
        }   
    }
    internal static void PlayVFX(string vfxName, Vector3 pos, Quaternion rot, Transform parent)
    {
        GameObject vfxObj = null;
        foreach (VFX vfx in Instance.vfxs)
        {
            if (vfx.vfxPrefab.name == vfxName)
            {
                vfxObj = Instantiate(vfx.vfxPrefab, pos, rot, parent);
                break;
            }
        }
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 10f;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            Instantiate(mouseClickVfx, worldPosition, Quaternion.identity);
            // Chơi âm thanh click
            SoundManager.PlaySfx("click");
        }    
    }
}
[System.Serializable]
public class VFX
{
    [SerializeField]
    internal Vfx vfxType;
    public GameObject vfxPrefab;
}
[System.Serializable]
public enum Vfx
{
    MOVE,
    CRASH,
}
