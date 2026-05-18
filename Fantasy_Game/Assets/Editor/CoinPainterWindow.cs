using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CoinPainterWindow : EditorWindow
{
    private GameObject coinPrefab;
    private Transform coinParent;
    private LayerMask groundLayer = ~0;

    private float spacing = 2f;
    private float heightOffset = 1f;
    private float lastPlaceDistance = 999f;

    private Vector3 lastPlacedPosition;
    private bool hasPlacedCoin = false;

    [MenuItem("Tools/Coin Painter")]
    public static void OpenWindow()
    {
        GetWindow<CoinPainterWindow>("Coin Painter");
    }

    // Draws the custom editor window
    private void OnGUI()
    {
        GUILayout.Label("Coin Painter", EditorStyles.boldLabel);

        coinPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Coin Prefab",
            coinPrefab,
            typeof(GameObject),
            false
        );

        coinParent = (Transform)EditorGUILayout.ObjectField(
            "Coin Parent",
            coinParent,
            typeof(Transform),
            true
        );

        groundLayer = EditorGUILayout.MaskField(
            "Ground Layer",
            groundLayer,
            InternalEditorUtility.layers
        );

        spacing = EditorGUILayout.FloatField("Coin Spacing", spacing);
        heightOffset = EditorGUILayout.FloatField("Height Offset", heightOffset);

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Hold SHIFT and left click/drag in the Scene view to paint coins onto the ground.",
            MessageType.Info
        );

        if (GUILayout.Button("Reset Paint Spacing"))
        {
            hasPlacedCoin = false;
            lastPlaceDistance = 999f;
        }
    }

    // Subscribes to Scene view input
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Unsubscribes from Scene view input
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    // Handles painting coins directly in the Scene view
    private void OnSceneGUI(SceneView sceneView)
    {
        Event currentEvent = Event.current;

        if (coinPrefab == null)
        {
            return;
        }

        if (!currentEvent.shift)
        {
            hasPlacedCoin = false;
            return;
        }

        if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag)
        {
            return;
        }

        if (currentEvent.button != 0)
        {
            return;
        }

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundLayer))
        {
            Vector3 placePosition = hit.point + Vector3.up * heightOffset;

            if (!hasPlacedCoin || Vector3.Distance(lastPlacedPosition, placePosition) >= spacing)
            {
                PlaceCoin(placePosition);
                lastPlacedPosition = placePosition;
                hasPlacedCoin = true;
            }

            currentEvent.Use();
        }
    }

    // Places a coin prefab into the scene
    private void PlaceCoin(Vector3 position)
    {
        GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(coinPrefab);

        if (coin == null)
        {
            return;
        }

        Undo.RegisterCreatedObjectUndo(coin, "Paint Coin");

        coin.transform.position = position;
        coin.transform.rotation = Quaternion.identity;

        if (coinParent != null)
        {
            coin.transform.SetParent(coinParent);
        }

        EditorUtility.SetDirty(coin);
    }
}