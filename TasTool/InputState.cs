using Rewired;
using System.Collections.Generic;

namespace TasTool;

public class InputState
{
    private readonly Dictionary<int, Button> _buttons = new();

    public InputState(Player player)
    {
        foreach (int buttonId in BUTTON_IDS)
            _buttons.Add(buttonId, new Button(player, buttonId));
    }

    public bool GetButton(int id) => _buttons[id].HasBit(0);
    public bool GetButtonDown(int id) => _buttons[id].HasBit(1);
    public bool GetButtonUp(int id) => _buttons[id].HasBit(2);
    public bool GetButtonPrev(int id) => _buttons[id].HasBit(3);

    private static readonly int[] BUTTON_IDS =
    {
        RewiredConsts.Action.Attack,
        RewiredConsts.Action.Dash,
    };
}

public struct Button
{
    private byte _state;

    public Button(Player input, int id)
    {
        SetBit(0, input.GetButton(id));
        SetBit(1, input.GetButtonDown(id));
        SetBit(2, input.GetButtonUp(id));
        SetBit(3, input.GetButtonPrev(id));
    }

    private void SetBit(int bit, bool value)
    {
        _state |= (byte)((value ? 1 : 0) << bit);
    }

    public readonly bool HasBit(int bit)
    {
        return (_state & bit) != 0;
    }
}

public struct Axis
{
    private float _state;
    private float _rawState;
}
