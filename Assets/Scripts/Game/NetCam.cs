using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NetCam : MonoBehaviourPunCallbacks
{
    public RenderTexture MainView;

    private void Start()
    {
        if (NetCore.I != null)
            NetCore.I.OnTransCam += SetImage;
        TransImage();
    }

    private void OnDestroy()
    {
        if (NetCore.I != null)
        NetCore.I.OnTransCam -= SetImage;
    }

    private void TransImage()
    {
        Texture2D tex = new Texture2D(MainView.width, MainView.height);
        RenderTexture.active = MainView;
        tex.ReadPixels(new Rect(0, 0, MainView.width, MainView.height), 0, 0);
        tex.Apply();
        byte[] tex_bytes = tex.EncodeToPNG();
        NetCore.I.GetImage(tex_bytes);
        Invoke("TransImage", 0.03f);
    }

    public void SetImage(byte[] tex_bytes)
    {
        Texture2D new_text = new Texture2D(MainView.width, MainView.height);
        new_text.LoadImage(tex_bytes);
        GetComponent<RawImage>().texture = new_text;
    }
}
