// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Sprites")]
    public Sprite defaultSprite;     // по умолчанию — вправо
    public Sprite verticalSprite;    // спрайт для движения вверх/вниз


    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0;
        rb.freezeRotation = true;

        if (sr != null && defaultSprite != null)
            sr.sprite = defaultSprite;
    }

    void Update()
    {
        movement = Vector2.zero;
        if (Input.GetKey("w")) movement += Vector2.up;
        if (Input.GetKey("s")) movement += Vector2.down;
        if (Input.GetKey("a")) movement += Vector2.left;
        if (Input.GetKey("d")) movement += Vector2.right;
        movement = movement.normalized * moveSpeed;

        // Управление спрайтом и разворотом
        if (movement.y != 0)
        {
            sr.sprite = verticalSprite;
            sr.flipX = false; // для вертикального лучше не флипать по X
            sr.flipY = movement.y > 0; // вверх — отзеркаливание
        }
        else if (movement.x != 0)
        {
            sr.sprite = defaultSprite;
            sr.flipY = false;
            sr.flipX = movement.x < 0; // влево — отзеркаливание по X
        }

        movement = Vector2.zero;
        if (Input.GetKey("w")) movement += Vector2.up;
        if (Input.GetKey("s")) movement += Vector2.down;
        if (Input.GetKey("a")) movement += Vector2.left;
        if (Input.GetKey("d")) movement += Vector2.right;
        movement = movement.normalized * moveSpeed;

        // Приоритет: сравниваем абсолютные значения осей
        if (Mathf.Abs(movement.y) > Mathf.Abs(movement.x))
        {
            // Вертикальное движение
            sr.sprite = verticalSprite;
            sr.flipX = false;
            sr.flipY = movement.y > 0;  // если вверх, зеркалим по Y
        }
        else if (Mathf.Abs(movement.x) > 0.01f)
        {
            // Горизонтальное движение
            sr.sprite = defaultSprite;
            sr.flipY = false;
            sr.flipX = movement.x < 0;   // если влево, зеркалим по X
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement);
    }
}
