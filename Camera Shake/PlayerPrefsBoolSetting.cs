//https://www.youtube.com/@AJGame-Dev

using UnityEngine;
using UnityEngine.Events;

public class PlayerPrefsBoolSetting : MonoBehaviour
{
    //This is the screenshake toggle code for the Camera Shake Youtube tutorial. 

    public string PlayerPrefsKey;

    [SerializeField] private UnityEngine.UI.Toggle _toggle;

    private void OnEnable()
    {
        _toggle.isOn = PlayerPrefs.GetInt(PlayerPrefsKey, 1) == 1;
    }

    public void Save(bool value)
    {
        PlayerPrefs.SetInt(PlayerPrefsKey, value ? 1 : 0);
        PlayerPrefs.Save();
        Config.OnChanged.Invoke();
    }

}

public class Config
{
    public static UnityEvent OnChanged = new ();
}
