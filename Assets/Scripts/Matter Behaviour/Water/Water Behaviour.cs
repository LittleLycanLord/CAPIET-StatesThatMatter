using LilLycanLord_Official;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class WaterBehaviour : MonoBehaviour
{
    //* ╔════════════╗
    //* ║ Components ║
    //* ╚════════════╝
    private BoxCollider2D waterCollider;

    //* ╔══════════╗
    //* ║ Displays ║
    //* ╚══════════╝
    [Header("Displays")]
    [SerializeField] private bool playerInWater = false;

    //* ╔════════╗
    //* ║ Fields ║
    //* ╚════════╝
    [Space(10)]
    [Header("Fields")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Sprite bubbleMask;
    [SerializeField] private int frontSortingOrder = 6;
    [SerializeField] private float buoyancyForce = 15f;
    [SerializeField] private float waterLinearDamping = 5f; // Replaces the player's drag while submerged
    [SerializeField] private float surfaceTrim = 0f; // Lowers the float target below the tile top edge; increase to stop the player appearing to hover above the surface

    //* ╔════════════╗
    //* ║ Attributes ║
    //* ╚════════════╝
    private Rigidbody2D playerRb;
    private PlatformerMovement playerMovement;
    private PolygonCollider2D[] tileColliders;
    private float originalLinearDamping;
    private float waterSurfaceY;

    //* ╔═══════════════╗
    //* ║ Monobehaviour ║
    //* ╚═══════════════╝
    void Awake()
    {
        waterCollider = GetComponent<BoxCollider2D>();
        waterCollider.isTrigger = true;
    }

    void Start()
    {
        // Must be Start — TilemapToGameObjects spawns TileSprite children during its own Start,
        // so children don't exist yet when Awake runs on this object.
        tileColliders = GetComponentsInChildren<PolygonCollider2D>();

        if (tileColliders.Length > 0)
        {
            waterSurfaceY = float.MinValue;
            foreach (var tile in tileColliders)
            {
                SpriteMask mask = tile.gameObject.AddComponent<SpriteMask>();
                mask.sprite = bubbleMask;
                mask.isCustomRangeActive = true;
                mask.frontSortingOrder = frontSortingOrder;
                mask.backSortingOrder = frontSortingOrder - 1;
                waterSurfaceY = Mathf.Max(waterSurfaceY, tile.bounds.max.y);
            }
        }
        else
        {
            waterSurfaceY = transform.position.y + waterCollider.offset.y + (waterCollider.size.y / 2f);
        }
    }

    void FixedUpdate()
    {
        if (!playerInWater || playerRb == null) return;

        if (playerRb.position.y < waterSurfaceY - surfaceTrim)
            playerRb.AddForce(Vector2.up * buoyancyForce, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        foreach (var tile in tileColliders)
            Physics2D.IgnoreCollision(other, tile, true);

        playerRb = other.attachedRigidbody;
        playerMovement = other.GetComponent<PlatformerMovement>();
        playerInWater = true;

        if (playerRb != null)
        {
            originalLinearDamping = playerRb.linearDamping;
            playerRb.linearDamping = waterLinearDamping;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        foreach (var tile in tileColliders)
            Physics2D.IgnoreCollision(other, tile, false);

        if (playerRb != null)
            playerRb.linearDamping = originalLinearDamping;

        playerInWater = false;
        playerRb = null;
        playerMovement = null;
    }

    void OnDestroy() {
        if (playerRb != null)
            playerRb.linearDamping = originalLinearDamping;
    }

    //* ╔═════════════════════╗
    //* ║ Non - Monobehaviour ║
    //* ╚═════════════════════╝

    //* ╔════════════════════════════════╗
    //* ║ Virtual / Overridden Functions ║
    //* ╚════════════════════════════════╝
}
