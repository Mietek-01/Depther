using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GlobalCursor : Singleton<GlobalCursor>
{
    public enum CursorType
    {
        START_MENU,
        GAMEPLAY,
        PAUSE_MENU,
    }

    [System.Serializable]
    struct Type
    {
        public CursorType cursorType;
        public Sprite sprite;
    }

    [SerializeField] CursorType defaultType = CursorType.START_MENU;
    [SerializeField] Type[] types = new Type[] { new Type { cursorType = CursorType.START_MENU } };

    public static Vector3 Position { get; private set; }

    public static bool VisibleInGameplay { get; set; } = true;

    public static bool Visible
    {
        get => instance.myRenderer.enabled;
        set => instance.myRenderer.enabled = value;
    }

    public static bool HigherSortingOrder
    {
        get => instance.myRenderer.sortingOrder != instance.basicSortingOrder;
        set => instance.myRenderer.sortingOrder = instance.basicSortingOrder * (value ? 2 : 1);
    }

    int basicSortingOrder;
    Vector3 mousePosition;

    Camera mainCamera;
    SpriteRenderer myRenderer;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        myRenderer = instance.GetComponent<SpriteRenderer>();
        mainCamera = GetComponentInParent<Camera>();

        if (mainCamera == null)
        {
            Debug.LogError("I must be a camera child");
            gameObject.SetActive(false);
            return;
        }

        Cursor.visible = false;

        mousePosition.z = -mainCamera.transform.position.z;

        basicSortingOrder = myRenderer.sortingOrder;

        SetCursorType(defaultType);
    }

    private void Update()
    {
        mousePosition.x = Input.mousePosition.x;
        mousePosition.y = Input.mousePosition.y;

        transform.position = mainCamera.ScreenToWorldPoint(mousePosition);
        Position = transform.position;
    }

    public static void SetCursorType(CursorType newType)
    {
        if (newType == CursorType.GAMEPLAY && !VisibleInGameplay)
            Visible = false;
        else
        {
            foreach (var type in instance.types)
                if (type.cursorType == newType)
                {
                    Visible = true;

                    if (type.sprite == null)
                        Debug.LogWarning("The sprite for type: " + newType + " is null");

                    instance.myRenderer.sprite = type.sprite;
                    return;
                }

            Debug.LogError("I didnt find the type: " + newType);

        }
    }

    public static void Freeze(bool freeze)
    {
        instance.enabled = !freeze;
    }
}
