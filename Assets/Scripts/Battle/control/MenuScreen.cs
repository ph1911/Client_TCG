using UnityEngine;

public class MenuScreen : MonoBehaviour
{
    public void CloseMenuScreen()
    {
        GameObject.Find("MenuScreen").GetComponent<Canvas>().enabled = false;
    }

    public void ReturnToMainMenu()
    {
        CloseMenuScreen();
        GameObject.Find("BattleScreen").GetComponent<Battle>().gameEnds("LEAVE");
    }

    public void QuitGame()
    {
        GameObject.Find("BattleScreen").GetComponent<Battle>().gameEnds("EXIT");
    }
}
