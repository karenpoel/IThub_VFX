using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField, Range(0.3f, 3f)] private float strength = 1f;
    [SerializeField] private float surfaceOffset = 0.1f;

    private void Start()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) strength = 0.6f;
            if (kb.digit2Key.wasPressedThisFrame) strength = 1f;
            if (kb.digit3Key.wasPressedThisFrame) strength = 2f;
        }

        bool clicked = mouse != null && mouse.leftButton.wasPressedThisFrame;
        Vector2 mousePos = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) strength = 0.6f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) strength = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) strength = 2f;

        bool clicked = Input.GetMouseButtonDown(0);
        Vector2 mousePos = Input.mousePosition;
#endif
        if (!clicked) return;

        // Если курсор заблокирован (управление персонажем), стреляем в центр экрана
        Vector2 screenPos = Cursor.lockState == CursorLockMode.Locked
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : mousePos;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hitMask,
                            QueryTriggerInteraction.Ignore))
        {
            ExplosionPool.Instance.Spawn(hit.point + hit.normal * surfaceOffset, strength);
        }
    }
}
