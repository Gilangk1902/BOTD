using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    public LineRenderer line;
    private Vector3 start;
    private Vector3 end;
    private float duration = 0.1f;
    private float time;

    public void Init(Vector3 startPos, Vector3 endPos, float trailDuration)
    {
        if (line == null) line = GetComponent<LineRenderer>();

        start = startPos;
        end = endPos;
        duration = trailDuration;
        time = 0f;

        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, start); // Mulai dari start, lalu Update akan menginterpolasi ke end
    }

    void Update()
    {
        time += Time.deltaTime;
        float t = time / duration;
        t = Mathf.Clamp01(t);

        Vector3 current = Vector3.Lerp(start, end, t);
        line.SetPosition(1, current);

        if (t >= 1f)
        {
            BulletTrailPool.Instance.ReturnTrail(gameObject);
        }
    }
}
