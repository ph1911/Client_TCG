using UnityEngine;

public class ResultCarrier : MonoBehaviour
{
    public static ResultCarrier Instance;
    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public int wins;
    public int losses;
    public string message;
}
