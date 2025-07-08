using System;
using Microsoft.Maui.Dispatching;
#if WINDOWS
using Windows.Gaming.Input;
#endif

namespace Xenia_360____Canary_.Services;

public class ControllerService
{
    private readonly IDispatcherTimer _timer;
    public event Action? OnHomeButtonPressed = delegate { };

#if WINDOWS
    private Gamepad? _gamepad;
#endif

    public ControllerService(IDispatcher dispatcher)
    {
#if WINDOWS
        Gamepad.GamepadAdded += Gamepad_GamepadAdded;
        Gamepad.GamepadRemoved += Gamepad_GamepadRemoved;
#endif

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

#if WINDOWS
    private void Gamepad_GamepadAdded(object? sender, Gamepad e)
    {
        _gamepad = e;
    }

    private void Gamepad_GamepadRemoved(object? sender, Gamepad e)
    {
        if (_gamepad == e)
            _gamepad = null;
    }
#endif

    private void Timer_Tick(object? sender, EventArgs e)
    {
#if WINDOWS
        if (_gamepad != null)
        {
            var reading = _gamepad.GetCurrentReading();

            // No GamepadButtons.Home in Windows.Gaming.Input, using View as a Home equivalent
            const GamepadButtons HomeEquivalent = GamepadButtons.View;

            if ((reading.Buttons & HomeEquivalent) == HomeEquivalent)
            {
                OnHomeButtonPressed?.Invoke();
            }
        }
#endif
    }
}
