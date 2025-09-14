using UnityEngine;
using UnityEngine.InputSystem; // <- IMPORTANT (nouveau système)

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    public float moveSpeed = 3f;
    public Vector2 boundsMin = new(-40,-40), boundsMax = new(40,40);
    public VirtualJoystick joystick;
    Rigidbody2D rb; Vector2 input;

    void Awake(){ rb = GetComponent<Rigidbody2D>(); Application.targetFrameRate = 60; }

    void Update(){
        if (joystick) {
            // Mobile / UI joystick
            input = joystick.Direction;
        } else {
            // Éditeur / PC : clavier + manette (New Input System)
            Vector2 v = Vector2.zero;

            var k = Keyboard.current;
            if (k != null) {
                float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                        - (k.aKey.isPressed || k.leftArrowKey.isPressed  ? 1f : 0f);
                float y = (k.wKey.isPressed || k.upArrowKey.isPressed    ? 1f : 0f)
                        - (k.sKey.isPressed || k.downArrowKey.isPressed  ? 1f : 0f);
                v = new Vector2(x, y);
            }

            var g = Gamepad.current;
            if (g != null) {
                // si un pad est branché, utilise le stick gauche (prioritaire s’il est plus fort)
                Vector2 stick = g.leftStick.ReadValue();
                if (stick.sqrMagnitude > v.sqrMagnitude) v = stick;
            }

            input = v;
        }
    }

    void FixedUpdate(){
        float mult = PlayerStats.Instance ? PlayerStats.Instance.moveSpeedMult : 1f;
        Vector2 target = rb.position + input.normalized * (moveSpeed * mult) * Time.fixedDeltaTime;
        target.x = Mathf.Clamp(target.x, boundsMin.x, boundsMax.x);
        target.y = Mathf.Clamp(target.y, boundsMin.y, boundsMax.y);
        rb.MovePosition(target);
    }
}
