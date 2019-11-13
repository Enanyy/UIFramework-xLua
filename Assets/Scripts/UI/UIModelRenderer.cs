using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class UIModelRenderer : MonoBehaviour
{
    private CanvasRenderer mCanvasRenderer;
    // Use this for initialization
    public GameObject target;

    public int sortingLayerID;
    public int sortingOrder;
    public int renderOrder;
    private void Start()
    {
        mCanvasRenderer = GetComponent<CanvasRenderer>();
        if(target)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            //Material material = mCanvasRenderer.GetMaterial();

            sortingLayerID = canvas.sortingLayerID;
            sortingOrder = canvas.sortingOrder;
            renderOrder = canvas.renderOrder;

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i)
            {
                renderers[i].sortingLayerID = canvas.sortingLayerID;
                renderers[i].sortingOrder = canvas.sortingOrder;
                renderers[i].material.renderQueue = 1000;
            }
        }
    }
    

    // Update is called once per frame
    void Update()
    {

    }
}
