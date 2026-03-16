using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class VaporBehaviour : MonoBehaviour
{
    //* ╔════════════╗
    //* ║ Components ║
    //* ╚════════════╝
    private BoxCollider2D vaporCollider;

    //* ╔══════════╗
    //* ║ Displays ║
    //* ╚══════════╝
    // [Header("Displays")]

    //* ╔════════╗
    //* ║ Fields ║
    //* ╚════════╝
    [Space(10)]
    [Header("Fields")]
    [SerializeField] private string[] passThroughTags = { "Player" };
    [SerializeField] private Sprite vaporMask;
    [SerializeField] private int frontSortingOrder = 6;

    //* ╔════════════╗
    //* ║ Attributes ║
    //* ╚════════════╝
    private PolygonCollider2D[] tileColliders;

    //* ╔═══════════════╗
    //* ║ Monobehaviour ║
    //* ╚═══════════════╝
    void Awake()
    {
        vaporCollider = GetComponent<BoxCollider2D>();
        vaporCollider.isTrigger = true;
    }

    void Start()
    {
        // Must be Start — TilemapToGameObjects spawns TileSprite children during its own Start,
        // so children don't exist yet when Awake runs on this object.
        tileColliders = GetComponentsInChildren<PolygonCollider2D>();

        foreach (var tile in tileColliders){
            SpriteMask mask = tile.gameObject.AddComponent<SpriteMask>();
            mask.sprite = vaporMask;
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = frontSortingOrder;
            mask.backSortingOrder = frontSortingOrder - 1;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasPassThroughTag(other)) return;

        foreach (var tile in tileColliders)
            Physics2D.IgnoreCollision(other, tile, true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!HasPassThroughTag(other)) return;

        foreach (var tile in tileColliders)
            Physics2D.IgnoreCollision(other, tile, false);
    }

    //* ╔═════════════════════╗
    //* ║ Non - Monobehaviour ║
    //* ╚═════════════════════╝
    private bool HasPassThroughTag(Collider2D other)
    {
        foreach (var tag in passThroughTags)
            if (other.CompareTag(tag)) return true;
        return false;
    }

    //* ╔════════════════════════════════╗
    //* ║ Virtual / Overridden Functions ║
    //* ╚════════════════════════════════╝
}
