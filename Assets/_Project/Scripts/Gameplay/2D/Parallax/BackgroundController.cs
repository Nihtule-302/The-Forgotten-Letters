using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private Vector2 startPos;
    private float length;
    public GameObject cam;
    public float parallaxEffect;

    [Tooltip("0 = move with camera, 1 = no movement")]
    public float ParallaxAmountX;
    public float ParallaxAmountY;
    public bool loop = true;

    private SpriteRenderer SpriteRenderer;
    private Bounds spriteBound;
    public Vector2 camStartPos;

    void Start()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        spriteBound = SpriteRenderer.localBounds;
        
        startPos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceX = (cam.transform.position.x - camStartPos.x) * ParallaxAmountX;
        float distanceY = (cam.transform.position.y - camStartPos.y) * ParallaxAmountY;

        float movementX = (cam.transform.position.x - camStartPos.x) * (1- ParallaxAmountX);

        transform.position = new Vector2(startPos.x + distanceX, startPos.y + distanceY);

        if (loop)
        {
            if (movementX > startPos.x + length)
            {
                startPos.x += length;
            }
            else if (movementX < startPos.x - length)
            {
                startPos.x -= length;
            } 
        }
    }
}
