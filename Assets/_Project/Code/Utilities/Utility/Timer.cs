using System;

public class Timer
{
    private float _duration;
    private float _elapsed;
    private bool _running;

    public bool IsDone => _running && _elapsed >= _duration;
    public bool IsRunning => _running;

    public bool IsComplete => _elapsed >= _duration;
    public Timer(float duration)
    {
        _duration = duration;
        _elapsed = 0f;
        _running = false;
    }

    public void Start()
    {
        _elapsed = 0f;
        _running = true;
    }

    public void Reset(float newDuration = -1f)
    {
        if (newDuration > 0f)
            _duration = newDuration;

        Start();
    }

    public void Stop()
    {
        _running = false;
    }

    public void TimerUpdate(float deltaTime)
    {
        if (!_running) return;

        _elapsed += deltaTime;
        if (_elapsed >= _duration)
        {
            _running = false;
        }
    }
}