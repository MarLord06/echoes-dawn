using UnityEngine;

public class InitializeCamera : MonoBehaviour
{

    [SerializeField] private Camera mainCam;

    private bool isInitialized;

    private void Awake()
    {
        mainCam.gameObject.SetActive(false);
        mainCam.gameObject.SetActive(true);
    }
}
