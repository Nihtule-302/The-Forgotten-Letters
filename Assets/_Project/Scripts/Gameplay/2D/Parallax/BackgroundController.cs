using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay._2D.Parallax
{
    public class BackgroundController : MonoBehaviour
    {
        public GameObject cam;
        public float parallaxEffect;

        [FormerlySerializedAs("ParallaxAmountX")] [Tooltip("0 = move with camera, 1 = no movement")]
        public float parallaxAmountX;

        [FormerlySerializedAs("ParallaxAmountY")] public float parallaxAmountY;
        public bool loop = true;
        public Vector2 camStartPos;
        private float _length;
        private Bounds _spriteBound;

        private SpriteRenderer _spriteRenderer;
        private Vector2 _startPos;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteBound = _spriteRenderer.localBounds;

            _startPos = transform.position;
            _length = GetComponent<SpriteRenderer>().bounds.size.x;

            camStartPos = cam.transform.position;
        }

        // Update is called once per frame
        private void Update()
        {
            var distanceX = (cam.transform.position.x - camStartPos.x) * parallaxAmountX;
            var distanceY = (cam.transform.position.y - camStartPos.y) * parallaxAmountY;

            var movementX = (cam.transform.position.x - camStartPos.x) * (1 - parallaxAmountX);

            transform.position = new Vector2(_startPos.x + distanceX, _startPos.y + distanceY);

            if (loop)
            {
                if (movementX > _startPos.x + _length)
                    _startPos.x += _length;
                else if (movementX < _startPos.x - _length) _startPos.x -= _length;
            }
        }
    }
}