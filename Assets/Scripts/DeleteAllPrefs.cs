using UnityEngine;

public class DeleteAllPrefs : MonoBehaviour
{
    public void DeletePrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
