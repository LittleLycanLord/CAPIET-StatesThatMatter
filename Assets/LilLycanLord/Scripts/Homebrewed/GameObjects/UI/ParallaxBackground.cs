using System;
using System.Collections.Generic;
using System.Linq;
using LilLycanLord_Official;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace LilLycanLord_Official
{
    [Serializable]
    public class ParallaxLayer
    {
        [HideInInspector]
        public string name;
        public GameObject layer;
        public Sprite sprite;
        public float parallaxEffect = 0.0f;

        [HideInInspector]
        public float startPosition;

        [HideInInspector]
        public float length;
    }

    [RequireComponent(typeof(Canvas))]
    public class ParallaxBackground : MonoBehaviour
    {
        //* ╔════════════╗
        //* ║ Components ║
        //* ╚════════════╝

        //* ╔══════════╗
        //* ║ Displays ║
        //* ╚══════════╝

        //* ╔════════╗
        //* ║ Fields ║
        //* ╚════════╝

        GameObject layerPool;
        Canvas canvas;

        [Header("Displays")]
        [SerializeField]
        bool backgroundsGenerated = false;

        [Space(10)]
        [Header("Fields")]
        [SerializeField]
        Camera trackedCamera;

        [SerializeField]
        float planeDistance = 100;

        [SerializeField]
        float layerZDistance = 10.0f;

        [SerializeField]
        Vector3 imageScale = new Vector3(1.0f, 1.0f, 1.0f);

        [Tooltip("The top-most Layer will be the farthest from the Camera.")]
        [SerializeField]
        List<ParallaxLayer> layers = new List<ParallaxLayer>();

        //* ╔════════════╗
        //* ║ Attributes ║
        //* ╚════════════╝

        //* ╔═══════════════╗
        //* ║ Monobehaviour ║
        //* ╚═══════════════╝

        void Awake() { }

        void Start()
        {
            GenerateParallaxBackground();
        }

        void LateUpdate()
        {
            canvas.planeDistance = planeDistance;
            foreach (ParallaxLayer layer in layers)
                UpdateLayer(layer);
        }

        //* ╔═══════════════════╗
        //* ║ Non-Monobehaviour ║
        //* ╚═══════════════════╝

        [ContextMenu("Generate Parallax Background")]
        public void GenerateParallaxBackground()
        {
            if (trackedCamera == null)
                trackedCamera = Camera.main;
            canvas = GetComponent<Canvas>();
            canvas.planeDistance = planeDistance;
            CreateLayerPool();

            if (backgroundsGenerated || layerPool.transform.childCount > 0)
            {
                Debug.LogWarning(
                    name
                        + " may have already been generated. Remove all of the\"Layers\"'s children."
                );
                backgroundsGenerated = true;
                return;
            }
            else
                backgroundsGenerated = false;

            foreach (ParallaxLayer layer in layers)
            {
                CreateLayer(layer);
                layer.name = "Layer " + layers.Count;
            }
            if (layers.Count > 0 && layers.Count >= 2)
            {
                layers[0].name = "Furthest Layer";
                layers[layers.Count - 1].name = "Closest Layer";
            }
            backgroundsGenerated = true;
            Debug.Log("Parallax Background Generated");
        }

        [ContextMenu("Reset Parallax Background")]
        public void ResetParallaxBackground()
        {
            if (!backgroundsGenerated)
                return;
            layerPool = transform.Find("Layers")?.gameObject;
            if (layerPool != null)
                DestroyImmediate(layerPool);
            CreateLayerPool();
            Debug.Log("Parallax Background Reset");
            backgroundsGenerated = false;
        }

        void CreateLayerPool()
        {
            layerPool = transform.Find("Layers")?.gameObject;
            if (layerPool == null)
            {
                layerPool = new GameObject("Layers");
                layerPool.transform.parent = transform;
                RectTransform rectTransform = layerPool.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2();
                rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                rectTransform.localPosition = new Vector3();
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                Debug.Log("Layer Pool created");
            }
        }

        void UpdateLayer(ParallaxLayer layer)
        {
            RectTransform rectTransform = layer.layer.GetComponent<RectTransform>();

            float distanceTraveled = trackedCamera.transform.position.x * layer.parallaxEffect;
            float distanceRemaining = trackedCamera.transform.position.x * -layer.parallaxEffect;

            float parallaxPosition = layer.startPosition + distanceTraveled;
            rectTransform.localPosition = new Vector3(
                parallaxPosition,
                rectTransform.localPosition.y,
                rectTransform.localPosition.z
            );

            if (distanceRemaining > layer.startPosition + layer.length)
                layer.startPosition += layer.length;
            else if (distanceRemaining < layer.startPosition - layer.length)
                layer.startPosition -= layer.length;
        }

        GameObject CreateLayer(ParallaxLayer parallaxLayer)
        {
            RectTransform newLayer = new GameObject(
                "Layer " + layerPool.transform.childCount
            ).AddComponent<RectTransform>();
            newLayer.SetParent(layerPool.transform);
            newLayer.anchorMin = new Vector2(0.0f, 0.0f);
            newLayer.anchorMax = new Vector2(1.0f, 1.0f);
            newLayer.localPosition = new Vector3(
                0.0f,
                0.0f,
                0.0f - (layerZDistance * layerPool.transform.childCount)
            );
            newLayer.sizeDelta = new Vector2(0.0f, 0.0f);
            newLayer.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            GameObject leftLayer = new GameObject("Left");
            GameObject middleLayer = new GameObject("Middle");
            GameObject rightLayer = new GameObject("Right");
            leftLayer.transform.parent = newLayer.transform;
            middleLayer.transform.parent = newLayer.transform;
            rightLayer.transform.parent = newLayer.transform;
            GameObject[] parallax = { leftLayer, middleLayer, rightLayer };

            foreach (GameObject layer in parallax)
            {
                Image image = layer.AddComponent<Image>();
                image.sprite = parallaxLayer.sprite;
                image.SetNativeSize();
                image.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                image.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);
                image.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
                image.rectTransform.localScale = imageScale;
                parallaxLayer.layer = layer;
                switch (layer.name)
                {
                    case "Left":
                        image.rectTransform.localPosition = new Vector2(
                            -GetComponent<RectTransform>().rect.width,
                            0.0f
                        );
                        break;
                    case "Right":
                        image.rectTransform.localPosition = new Vector2(
                            GetComponent<RectTransform>().rect.width,
                            0.0f
                        );
                        break;
                    case "Middle":
                    default:
                        image.rectTransform.localPosition = new Vector2(0.0f, 0.0f);
                        break;
                }
                parallaxLayer.length = GetComponent<RectTransform>().rect.width;
            }
            parallaxLayer.startPosition = 0.0f;
            parallaxLayer.layer = newLayer.gameObject;

            return newLayer.gameObject;
        }

        //* ╔════════════════════════════════╗
        //* ║ Virtual / Overridden Functions ║
        //* ╚════════════════════════════════╝
    }
}
