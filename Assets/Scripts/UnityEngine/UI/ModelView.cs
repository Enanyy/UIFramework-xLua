using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class ModelView : MonoBehaviour
    {
        public RawImage targetImage;
        public Camera modelCamera;
        public GameObject modelRoot;

        public int modelLayer = 6;

        private RenderTexture mRenderTexture;
        // Use this for initialization


        void Start()
        {
            InitRenderTexture();
        }

        // Update is called once per frame
        void OnDestroy()
        {
            ReleaseRenderTexture();
        }

        private void InitRenderTexture()
        {
            SetLayer(modelRoot, modelLayer);

            if (modelCamera && targetImage)
            {
                mRenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
                modelCamera.targetTexture = mRenderTexture;
                modelCamera.cullingMask = 1 << modelLayer;
               
                targetImage.texture = mRenderTexture;
                
            }
        }

        private void ReleaseRenderTexture()
        {
            if (modelCamera) modelCamera.targetTexture = null;
            if (targetImage) targetImage.texture = null;

            RenderTexture.ReleaseTemporary(mRenderTexture);
        }

        private void SetLayer(GameObject go, int layer)
        {
            if (go == null || go.layer == layer)
            {
                return;
            }
            go.layer = layer;

            var transforms = go.GetComponentsInChildren<Transform>();
            for (int i = 0; i < transforms.Length; ++i)
            {
                transforms[i].gameObject.layer = layer;
            }
        }
    }
}
