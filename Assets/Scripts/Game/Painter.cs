using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class IntPoint
{
    public byte X;
    public byte Y;

    public IntPoint(byte x, byte y)
    {
        X = x;
        Y = y;
    }
}

public class Painter : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public static Painter I;

    public GameObject Fade;

    public Color[] ColorsTable = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
    public Color PenColour = Color.white;

    public RectTransform SelfTransform;
    public Texture2D Texture;
    public Color32 ResetColour = Color.clear;

    private Color32[] _cleanColoursArray;
    private Color32[] _curColors;

    public Action OnDrawBegin;
    public Action OnDrawEnd;

    public bool enable_draw = true;

    private void Awake()
    {
        I = this;
    }

    private void Start()
    {
        if (NetCore.I != null)
            NetCore.I.OnClearPrint += ClearPrint;

        I.Fade = transform.Find("Fade").gameObject;
        _cleanColoursArray = new Color32[Texture.width * Texture.height];        
        for (int x = 0; x < _cleanColoursArray.Length; x++)
            _cleanColoursArray[x] = ResetColour;

        ResetCanvas();
    }

    public void ResetCanvas()
    {
        _curColors = new Color32[Texture.width * Texture.height];
        Array.Copy(_cleanColoursArray, _curColors, _curColors.Length);
        Texture.SetPixels32(_cleanColoursArray);
        Texture.Apply();
    }

    public void ClearPrint(bool enable_draw)
    {
        this.enable_draw = enable_draw;
        Painter.I.ResetCanvas();
        if (!enable_draw)
            I.Fade.SetActive(true);
        else
            I.Fade.SetActive(false);
    }

    private Vector2 GetPointInTexture(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfTransform, eventData.position, eventData.pressEventCamera, out var point))
        {
            point += new Vector2(Texture.width / 2, Texture.height / 2);
            return point;
        }
        return new Vector2(-1, -1);
    }

    private static Vector2 NegVector = new Vector2(-1, -1);
    private Vector2 _lastPoint = NegVector;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _lastPoint = NegVector;
        OnDrawBegin?.Invoke();
        ResetCanvas();        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (enable_draw)
        {
            var point = GetPointInTexture(eventData);
            if (point.x < Texture.width && point.y < Texture.height && point.x >= 0 && point.y >= 0)
            {
                if (_lastPoint != NegVector)
                {
                    ColourBetween(_lastPoint, point, 2, PenColour);
                }
                else
                {
                    MarkPixelsToColour(point, 2, PenColour);
                }
                ApplyMarkedPixelChanges();
                _lastPoint = point;
            }
            else
            {
                _lastPoint = NegVector;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enable_draw)
        {
            _lastPoint = NegVector;
            OnDrawEnd?.Invoke();
        }
    }

    private void ColourBetween(Vector2 start_point, Vector2 end_point, int width, Color color)
    {
        var distance = Vector2.Distance(start_point, end_point);
        Vector2 cur_position;

        var lerp_steps = 1f / distance;

        for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
        {
            cur_position = Vector2.Lerp(start_point, end_point, lerp);
            MarkPixelsToColour(cur_position, width, color);
        }
    }

    private void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = (int)center_pixel.x;
        int center_y = (int)center_pixel.y;
        //int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= Texture.width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen);
            }
        }
    }

    private void MarkPixelsToColour(IntPoint center_pixel, int pen_thickness, Color color_of_pen, int width, int height, Color32[] pic)
    {
        // Figure out how many pixels we need to colour in each direction (x and y)
        int center_x = center_pixel.X;
        int center_y = center_pixel.Y;
        //int extra_radius = Mathf.Min(0, pen_thickness - 2);

        for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
        {
            // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
            if (x >= width || x < 0)
                continue;

            for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
            {
                MarkPixelToChange(x, y, color_of_pen, width, pic);
            }
        }
    }

    private void MarkPixelToChange(int x, int y, Color color, int width, Color32[] pic)
    {
        // Need to transform x and y coordinates to flat coordinates of array
        int array_pos = y * width + x;

        // Check if this is a valid position
        if (array_pos > pic.Length || array_pos < 0)
            return;

        pic[array_pos] = color;
    }

    private void MarkPixelToChange(int x, int y, Color color)
    {
        var arrayPos = y * Texture.width + x;

        // Check if this is a valid position
        if (arrayPos > _curColors.Length || arrayPos < 0)
            return;

        _curColors[arrayPos] = color;
    }

    private void ApplyMarkedPixelChanges()
    {
        Texture.SetPixels32(_curColors);
        Texture.Apply();
    }

    public Texture2D RenderTex(byte color, List<IntPoint> points)
    {
        var tex = new Texture2D(Texture.width, Texture.height);
        tex.SetPixels32(_cleanColoursArray);
        tex.anisoLevel = 0;
        var pic = tex.GetPixels32();
        for (var i = 0; i < points.Count; ++i)
        {
            MarkPixelsToColour(points[i], 1, ColorsTable[color], Texture.width, Texture.height, pic);
        }

        tex.SetPixels32(pic);
        tex.Apply();
        return tex;
    }

    public Texture2D GetDraw()
    {
        var tex = new Texture2D(Texture.width, Texture.height)
        {
            anisoLevel = 0
        };
        tex.SetPixels32(_curColors);
        tex.Apply();
        return tex;
    }

    public void OnDestroy()
    {
        if (NetCore.I != null)
            NetCore.I.OnClearPrint -= ClearPrint;
    }
}
