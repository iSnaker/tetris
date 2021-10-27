using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Magic : MonoBehaviour
{
    public Transform Root;
    public Figure FigurePrefab;
    public Material[] Materials;
    
    public List<Figure> Items = new List<Figure>();

    public bool enable_spawn = true;

    public void Awake()
    {
        if (NetCore.I == null)
        {
            SceneManager.LoadScene("Loader");
            return;
        }
    }
    void Start()
    {
        if (NetCore.I != null)
        {
            NetCore.I.OnLeftRoomAction += OnPlayerLeftRoom;
            NetCore.I.OnFigureRecieveAction += OnFigureRecieve;
        }
        if (Painter.I != null)
        {
            Painter.I.OnDrawBegin += OnDrawBegin;
            Painter.I.OnDrawEnd += OnDrawEnd;
        }
    }

    public void Clear()
    {
        for (var i = 0; i < Items.Count; ++i)
        {
            Destroy(Items[i].gameObject);
        }
        Items.Clear();
    }

    public void OnDestroy()
    {
        if (NetCore.I != null)
        {
            NetCore.I.OnLeftRoomAction += OnPlayerLeftRoom;
            NetCore.I.OnFigureRecieveAction -= OnFigureRecieve;
        }

        if (Painter.I != null)
        {
            Painter.I.OnDrawEnd -= OnDrawEnd;
            Painter.I.OnDrawBegin -= OnDrawBegin;
        }
    }

    public void OnDrawBegin()
    {

    }

    private readonly List<IntPoint> _lFigure = new List<IntPoint>();

    public void OnDrawEnd()
    {
        var tex = Painter.I.GetDraw();
        PrepareTexture(tex, Painter.I.PenColour);
        var color = (byte)Random.Range(0, Painter.I.ColorsTable.Length);
        NetCore.I.SendFigure(new FigurePacket(PlayerData.I.UserId, color, _lFigure));        
    }

    public void OnFigureRecieve(FigurePacket fig)
    {
        NetCore.I.ClearPrint(false);
        enable_spawn = false;

        var tex = Painter.I.RenderTex(fig.Color, fig.Points);
        PrepareTexture(tex, Painter.I.ColorsTable[fig.Color], false);

        Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

        var go = Instantiate(FigurePrefab, Root);
        go.GetComponent<Figure>().Color = fig.Color;
        go.transform.localPosition = new Vector3(0, 0, -20);
        go.SpriteRenderer.sprite = sp;
        go.gameObject.AddComponent(typeof(PolygonCollider2D));
        go.SpriteRenderer.material = Materials[fig.Color];
        go.Init();
        Items.Add(go);
    }

    private void PrepareTexture(Texture2D tex, Color penColor, bool fullCreate = true)
    {
        var width = tex.width;
        var height = tex.height;
        var topY = 0;
        var bottomY = height - 1;
        var leftX = 0;
        var rightX = width - 1;

        var pixels = tex.GetPixels32();

        // Trim
        for (var y = 0; y < height; ++y)
        {
            topY = y;
            var cleanLine = true;
            for (var x = 0; x < width; ++x)
            {
                var arrayPos = y * width + x;
                var pixel = pixels[arrayPos];
                if (pixel.a > 0 && pixel != Color.black)
                {
                    cleanLine = false;
                    break;
                }
            }
            if (!cleanLine)
                break;
        }

        for (var y = height - 1; y >= 0; --y)
        {
            bottomY = y;
            var cleanLine = true;
            for (var x = 0; x < width; ++x)
            {
                var arrayPos = y * width + x;
                var pixel = pixels[arrayPos];
                if (pixel.a > 0 && pixel != Color.black)
                {
                    cleanLine = false;
                    break;
                }
            }
            if (!cleanLine)
                break;
        }

        for (var x = 0; x < width; ++x)
        {
            leftX = x;
            var cleanLine = true;
            for (var y = 0; y < height; ++y)
            {
                var arrayPos = y * width + x;
                var pixel = pixels[arrayPos];
                if (pixel.a > 0 && pixel != Color.black)
                {
                    cleanLine = false;
                    break;
                }
            }
            if (!cleanLine)
                break;
        }

        for (var x = width - 1; x >= 0; --x)
        {
            rightX = x;
            var cleanLine = true;
            for (var y = 0; y < height; ++y)
            {
                var arrayPos = y * width + x;
                var pixel = pixels[arrayPos];
                if (pixel.a > 0 && pixel != Color.black)
                {
                    cleanLine = false;
                    break;
                }
            }
            if (!cleanLine)
                break;
        }

        if (fullCreate)
        {
            // Get Line data
            _lFigure.Clear();

            pixels = tex.GetPixels32();
            var n = pixels.Length;
            byte bpx = 0;
            byte bpy = 0;
            for (var i = 0; i < n; ++i)
            {
                var p = pixels[i];
                if (p.a > 0 && p != Color.black)
                {
                    _lFigure.Add(new IntPoint(bpx, bpy));
                }
                bpx++;
                if (bpx >= width)
                {
                    bpx = 0;
                    bpy++;
                }
            }
        }
        // FloodFill        

        for (var y = 0; y < height; ++y)
        {
            var arrayPos = y * width + 0;
            pixels[arrayPos] = Color.clear;
            arrayPos = y * width + width - 1;
            pixels[arrayPos] = Color.clear;
        }

        for (var x = 0; x < width; ++x)
        {
            var arrayPos = 0 * width + x;
            pixels[arrayPos] = Color.clear;
            arrayPos = (height - 1) * width + x;
            pixels[arrayPos] = Color.clear;
        }

        tex.SetPixels32(pixels);
        tex.Apply();

        tex.FloodFillBorder(0, 0, Color.black, penColor);
        tex.Apply();

        pixels = tex.GetPixels32();
        var px = 0;
        var py = 0;
        for (var i = 0; i < pixels.Length; ++i)
        {

            if (pixels[i].a == 0)
            {
                tex.FloodFillBorder(px, py, penColor, penColor);
                tex.Apply();
                pixels = tex.GetPixels32();
            }
            px++;
            if (px >= width)
            {
                px = 0;
                py++;
                ;
            }
        }


        px = 0;
        py = 0;
        for (var i = 0; i < pixels.Length; ++i)
        {
            px++;
            if (px >= width)
            {
                px = 0;
                py++;

            }
            if (pixels[i] == Color.black)
            {
                tex.FloodFillBorder(px, py, Color.clear, penColor);
                tex.Apply();
                pixels = tex.GetPixels32();
            }
        }

        // Copy data
        var newW = rightX - leftX + 1;
        var newH = bottomY - topY + 1;
        var pixels2 = new Color32[newW * newH];
        for (int x = 0; x < pixels2.Length; x++)
            pixels2[x] = Painter.I.ResetColour;

        var x2 = 0;
        for (var x = leftX; x <= rightX; ++x)
        {
            var y2 = 0;
            for (var y = topY; y <= bottomY; ++y)
            {
                var arrayPos = y * width + x;
                var arrayPos2 = y2 * newW + x2;

                pixels2[arrayPos2] = pixels[arrayPos];
                y2++;
            }
            x2++;
        }

        tex.Resize(newW, newH);
        tex.SetPixels32(pixels2);
        tex.Apply();
    }

    public void OnExitRoomClick()
    {
        NetCore.I.LeaveRoom();
    }

    public void OnPlayerLeftRoom()
    {
        if (NetCore.I.InRoom)
        {
            NetCore.I.LeaveRoom();
        }
        else
        { 
            SceneManager.LoadScene("Loader");
        }
    }
}
