using UnityEngine;
using UnityEngine.UI;

public class Figure : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    private Rigidbody2D rb;
    public byte Color;
    private bool dynamic = true, single = true;

    public Text Label;

    public void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        Label.text = (rb.mass * 10).ToString("0.00");
        GetComponent<Score>().score = rb.mass * 10;
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && dynamic)
        {
            if (mousePos.x < 0)
                rb.velocity = new Vector2(rb.velocity.x - 3f, rb.velocity.y);
            else
                rb.velocity = new Vector2(rb.velocity.x + 3f, rb.velocity.y);
        }
        else if (Input.GetMouseButtonUp(0) && dynamic)
            rb.velocity = new Vector2(0f, rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dynamic && collision.transform.tag != "Wall")
        {
            dynamic = false;
            GameObject.Find("Dropper").GetComponent<Magic>().enable_spawn = true;
            NetCore.I.ClearPrint(true);
        }

        if (collision.transform.tag == "Untagged" && GetComponentInChildren<Figure>().Color == collision.gameObject.GetComponentInChildren<Figure>().Color && GetComponent<Score>().score + collision.gameObject.GetComponent<Score>().score >= 10)
        {
            GameObject.Find("Dropper").GetComponent<ScoreManager>().AddScore(GetComponent<Score>().score);
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }

        if (single && collision.transform.name == "FigurePrefab(Clone)")
        { 
            if (collision.gameObject.GetComponent<Figure>().single && Color == collision.gameObject.GetComponent<Figure>().Color)
            {
                single = false;
                GameObject Merge = Instantiate(new GameObject("Merge"), GameObject.Find("Root").transform);
                Merge.AddComponent<Rigidbody2D>();
                Merge.AddComponent<Score>();
                Merge.GetComponent<Score>().score = GetComponent<Score>().score + collision.gameObject.GetComponent<Score>().score;
                Merge.GetComponent<Rigidbody2D>().gravityScale = GetComponent<Rigidbody2D>().gravityScale;
                gameObject.transform.SetParent(Merge.transform);
                collision.transform.SetParent(Merge.transform);
                Destroy(gameObject.GetComponent<Rigidbody2D>());
                Destroy(collision.gameObject.GetComponent<Rigidbody2D>());
                Destroy(gameObject.GetComponent<Score>());
                Destroy(collision.gameObject.GetComponent<Score>());
                Destroy(GameObject.Find("Merge"));
            }
        }
        else if (collision.transform.name == "Merge(Clone)" && Color == collision.gameObject.GetComponentInChildren<Figure>().Color)
        {
            collision.gameObject.GetComponent<Score>().score += GetComponent<Score>().score;
            transform.SetParent(collision.transform);
            Destroy(gameObject.GetComponent<Rigidbody2D>());
            Destroy(gameObject.GetComponent<Score>());
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!transform.GetComponentInChildren<Figure>().dynamic)
            GameObject.Find("Dropper").GetComponent<ScoreManager>().EndGame(new object[] { 0, 1, 0 });
    }
}
