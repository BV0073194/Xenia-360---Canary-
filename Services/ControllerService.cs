using System;
using Microsoft.Maui.Dispatching;
#if WINDOWS
using Windows.Gaming.Input;
#endif

namespace Xenia_360____Canary_.Services;

public class ControllerService
{
    private readonly IDispatcherTimer _timer;
    public event Action? OnHomeButtonPressed;

#if WINDOWS
    private Gamepad? _gamepad;
    private bool _isHomeButtonDown = false;
#endif

    public ControllerService(IDispatcher dispatcher)
    {
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(100);
        _timer.Tick += Timer_Tick;

#if WINDOWS
        Gamepad.GamepadAdded += (s, e) => { _gamepad = Gamepad.Gamepads.FirstOrDefault(); };
        Gamepad.GamepadRemoved += (s, e) => { _gamepad = Gamepad.Gamepads.FirstOrDefault(); };
        _gamepad = Gamepad.Gamepads.FirstOrDefault();
#endif
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
#if WINDOWS
        if (_gamepad != null)
        {
            var reading = _gamepad.GetCurrentReading();
            bool isPressed = (reading.Buttons & GamepadButtons.View) == GamepadButtons.View; // Using "View" button as Home

            if (isPressed && !_isHomeButtonDown)
            {
                OnHomeButtonPressed?.Invoke();
            }
            _isHomeButtonDown = isPressed;
        }
#endif
    }
}