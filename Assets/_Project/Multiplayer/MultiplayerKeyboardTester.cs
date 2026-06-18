using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerKeyboardTester : MonoBehaviour
{
    public LanConnectionManager lanConnectionManager;
    public NetworkBlackjackTable networkBlackjackTable;

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            lanConnectionManager.StartHost();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            networkBlackjackTable.StartRoundButton();
        }

        // Player 1
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugShowPlayerCards(0);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugHitPlayer(0);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugStandPlayer(0);
        }

        // Player 2
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugShowPlayerCards(1);
        }

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugHitPlayer(1);
        }

        if (Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugStandPlayer(1);
        }

        // Player 3
        if (Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugShowPlayerCards(2);
        }

        if (Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugHitPlayer(2);
        }

        if (Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            networkBlackjackTable.DebugStandPlayer(2);
        }
    }
}