using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningPopup : MonoBehaviour
{
    public Page page;
    public void DeleteClicked()
    {
        page.deletionConfirmed = true;
        Destroy(gameObject);
    }
    
    public void CloseWindow()
    {
        page.cancelConfirmed = true;
        Destroy(gameObject);
    }
}
