// ProgrammingOverlayBotHolder.cs
using UnityEngine;
using Bot.Programming;

public class ProgrammingOverlayBotHolder : MonoBehaviour
{
    [SerializeField] private BotProgrammingController botProgramming;
    public BotProgrammingController BotProgramming => botProgramming;

    public void Set(BotProgrammingController value)
    {
        botProgramming = value;
        Debug.Log(value
            ? $"[BotHolder] Set BotProgramming='{value.name}' on holder '{name}'"
            : $"[BotHolder] Cleared BotProgramming on holder '{name}'");
    }
}