using UnityEngine;

/// <summary>Moves rigid books into the title sculpture; the meshes are baked in the scene.</summary>
public sealed class TitleBookSculpture : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _composition;
    [SerializeField] private Transform _orbit;
    [SerializeField] private Transform[] _books;
    [SerializeField] private Vector2 _framingSize = new Vector2(12.2f, 10.8f);
    [SerializeField, Min(.1f)] private float _assemblyDuration = 1.5f;
    [SerializeField, Min(0)] private float _bookDelay = .055f;
    private Vector3[] _positions;
    private Quaternion[] _rotations;
    private float _elapsed, _aspect;
    private bool _reduced, _assembled;

    private void Awake()
    {
        _positions = new Vector3[_books.Length];
        _rotations = new Quaternion[_books.Length];
        for (int i = 0; i < _books.Length; i++)
        {
            _positions[i] = _books[i].localPosition;
            _rotations[i] = _books[i].localRotation;
        }
    }

    private void OnEnable()
    {
        _elapsed = 0;
        _reduced = GamePreferences.ReducedMotion;
        _assembled = _reduced;
        GamePreferences.Changed += OnPreferencesChanged;
        Frame(_camera.aspect);
        ApplyPose();
    }

    private void OnDisable()
    {
        GamePreferences.Changed -= OnPreferencesChanged;
        // Do not leave displaced books behind when a component is toggled off.
        if (_positions == null) return;
        SettleBooks();
        _orbit.localPosition = Vector3.zero;
        _orbit.localRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(_aspect, _camera.aspect)) Frame(_camera.aspect);
        if (_reduced) return;
        _elapsed += Time.unscaledDeltaTime;
        ApplyPose();
    }

    /// <summary>Keep the sculpture in the right-hand area at different window sizes.</summary>
    public void Frame(float aspect)
    {
        _aspect = aspect;
        float height = _camera.orthographicSize * 2;
        float width = height * aspect;
        float scale = Mathf.Min(width * .60f / _framingSize.x, height * .90f / _framingSize.y);
        _composition.localScale = Vector3.one * scale;
        _composition.position = _camera.transform.position + _camera.transform.forward * 20
            + _camera.transform.right * (width * .19f);
    }

    private void ApplyPose()
    {
        if (_positions == null) return;
        if (_reduced)
        {
            SettleBooks();
            _orbit.localPosition = Vector3.zero;
            _orbit.localRotation = Quaternion.identity;
            return;
        }

        if (!_assembled)
        {
            for (int i = 0; i < _books.Length; i++)
            {
                float t = Mathf.Clamp01((_elapsed - i * _bookDelay) / _assemblyDuration);
                float remaining = 1 - t;
                float ease = remaining * remaining * remaining;
                // Only translation and rotation. Every cover and page block stays rigid.
                _books[i].localPosition = _positions[i] + Vector3.back * (2.4f * ease);
                _books[i].localRotation = _rotations[i];
            }
            _assembled = _elapsed >= _assemblyDuration + (_books.Length - 1) * _bookDelay;
        }
        else
        {
            // The final three books occasionally leave their places and return in order.
            // Their fragments travel with the books, then reconnect at the resting pose.
            float phase = Mathf.Repeat(_elapsed - _assemblyDuration - (_books.Length - 1) * _bookDelay, 22);
            for (int j = 0; j < Mathf.Min(3, _books.Length); j++)
            {
                int i = _books.Length - 3 + j;
                float leave = Mathf.SmoothStep(0, 1, (phase - 4 - j * 2.2f) / 2);
                float arrive = Mathf.SmoothStep(0, 1, (phase - 12 - (2 - j) * 2.2f) / 2);
                float amount = leave * (1 - arrive);
                float clear = Mathf.SmoothStep(0, 1, amount / .22f);
                float travel = Mathf.SmoothStep(0, 1, (amount - .22f) / .78f);
                var queue = new Vector3(2.5f + j * .9f, -3.6f + j * .15f, -2.2f - j * .6f);
                var pulled = _positions[i] + Vector3.back * (2.2f * clear);
                _books[i].localPosition = Vector3.Lerp(pulled, queue, travel);
                _books[i].localRotation = Quaternion.Slerp(_rotations[i], Quaternion.Euler(0, -12 + j * 8, 0), travel);
            }
        }

        _orbit.localPosition = Vector3.up * (Mathf.Sin(_elapsed * .4f) * .045f);
        _orbit.localRotation = Quaternion.Euler(Mathf.Sin(_elapsed * .19f) * .5f,
            Mathf.Sin(_elapsed * .23f) * .9f, Mathf.Sin(_elapsed * .17f) * .35f);
    }

    private void SettleBooks()
    {
        for (int i = 0; i < _books.Length; i++)
        {
            _books[i].localPosition = _positions[i];
            _books[i].localRotation = _rotations[i];
        }
        _assembled = true;
    }

    private void OnPreferencesChanged()
    {
        bool reduced = GamePreferences.ReducedMotion;
        if (_reduced == reduced) return;
        _reduced = reduced;
        // Switching back to normal motion resumes the idle pose, without replaying the intro.
        SettleBooks();
        ApplyPose();
    }
}
