using System.Collections.Concurrent;
using Lanterna.Core;

namespace Lanterna.Gui2;

/// <summary>
/// This is a special label that contains not just a single text to display but a number of frames that are cycled
/// through. The class will manage a timer on its own and ensure the label is updated and redrawn. There is a static
/// helper method available to create the classic "spinning bar": CreateClassicSpinningLine()
/// </summary>
public class AnimatedLabel : Label
{
    private static Timer? _timer;
    private static readonly ConcurrentDictionary<AnimatedLabel, TimerTask> ScheduledTasks = new();
    private static readonly object TimerLock = new object();

    /// <summary>
    /// Creates a classic spinning bar which can be used to signal to the user that an operation is in process.
    /// </summary>
    /// <returns>AnimatedLabel instance which is setup to show a spinning bar</returns>
    public static AnimatedLabel CreateClassicSpinningLine()
    {
        return CreateClassicSpinningLine(150);
    }

    /// <summary>
    /// Creates a classic spinning bar which can be used to signal to the user that an operation is in process.
    /// </summary>
    /// <param name="speed">Delay in between each frame</param>
    /// <returns>AnimatedLabel instance which is setup to show a spinning bar</returns>
    public static AnimatedLabel CreateClassicSpinningLine(int speed)
    {
        var animatedLabel = new AnimatedLabel("-");
        animatedLabel.AddFrame("\\");
        animatedLabel.AddFrame("|");
        animatedLabel.AddFrame("/");
        animatedLabel.StartAnimation(speed);
        return animatedLabel;
    }

    private readonly List<string[]> _frames;
    private TerminalSize _combinedMaximumPreferredSize;
    private int _currentFrame;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new animated label, initially set to one frame. You will need to add more frames and call
    /// StartAnimation() for this to start moving.
    /// </summary>
    /// <param name="firstFrameText">The content of the label at the first frame</param>
    public AnimatedLabel(string firstFrameText) : base(firstFrameText)
    {
        _frames = new List<string[]>();
        _currentFrame = 0;
        _combinedMaximumPreferredSize = TerminalSize.Zero;

        string[] lines = SplitIntoMultipleLines(firstFrameText);
        _frames.Add(lines);
        EnsurePreferredSize(lines);
    }

    protected override TerminalSize CalculatePreferredSize()
    {
        lock (_lock)
        {
            return base.CalculatePreferredSize().Max(_combinedMaximumPreferredSize);
        }
    }

    /// <summary>
    /// Adds one more frame at the end of the list of frames
    /// </summary>
    /// <param name="text">Text to use for the label at this frame</param>
    /// <returns>Itself</returns>
    public AnimatedLabel AddFrame(string text)
    {
        lock (_lock)
        {
            string[] lines = SplitIntoMultipleLines(text);
            _frames.Add(lines);
            EnsurePreferredSize(lines);
            return this;
        }
    }

    private void EnsurePreferredSize(string[] lines)
    {
        _combinedMaximumPreferredSize = _combinedMaximumPreferredSize.Max(GetBounds(lines, _combinedMaximumPreferredSize));
    }

    /// <summary>
    /// Advances the animated label to the next frame. You normally don't need to call this manually as it will be done
    /// by the animation thread.
    /// </summary>
    public void NextFrame()
    {
        lock (_lock)
        {
            _currentFrame++;
            if (_currentFrame >= _frames.Count)
            {
                _currentFrame = 0;
            }
            SetLines(_frames[_currentFrame]);
            Invalidate();
        }
    }

    public override void OnRemoved(IContainer container)
    {
        StopAnimation();
        base.OnRemoved(container);
    }

    /// <summary>
    /// Starts the animation thread which will periodically call NextFrame() at the interval specified by the
    /// millisecondsPerFrame parameter. After all frames have been cycled through, it will start over from the
    /// first frame again.
    /// </summary>
    /// <param name="millisecondsPerFrame">The interval in between every frame</param>
    /// <returns>Itself</returns>
    public AnimatedLabel StartAnimation(long millisecondsPerFrame)
    {
        lock (TimerLock)
        {
            if (_timer == null)
            {
                _timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            }

            var animationTask = new TimerTask(this, millisecondsPerFrame);
            ScheduledTasks[this] = animationTask;
            animationTask.Start();
            return this;
        }
    }

    /// <summary>
    /// Halts the animation thread and the label will stop at whatever was the current frame at the time when this was
    /// called
    /// </summary>
    /// <returns>Itself</returns>
    public AnimatedLabel StopAnimation()
    {
        RemoveTaskFromTimer(this);
        return this;
    }

    private static void RemoveTaskFromTimer(AnimatedLabel animatedLabel)
    {
        lock (TimerLock)
        {
            if (ScheduledTasks.TryRemove(animatedLabel, out var task))
            {
                task.Cancel();
            }
            CanCloseTimer();
        }
    }

    private static void CanCloseTimer()
    {
        if (ScheduledTasks.IsEmpty)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    private static void TimerCallback(object? state)
    {
        // This is handled by individual TimerTask instances
    }

    private class TimerTask
    {
        private readonly WeakReference<AnimatedLabel> _labelRef;
        private readonly long _millisecondsPerFrame;
        private Timer? _individualTimer;
        private bool _cancelled;

        public TimerTask(AnimatedLabel label, long millisecondsPerFrame)
        {
            _labelRef = new WeakReference<AnimatedLabel>(label);
            _millisecondsPerFrame = millisecondsPerFrame;
            _cancelled = false;
        }

        public void Start()
        {
            if (!_cancelled)
            {
                _individualTimer = new Timer(Execute, null, _millisecondsPerFrame, _millisecondsPerFrame);
            }
        }

        public void Cancel()
        {
            _cancelled = true;
            _individualTimer?.Dispose();
            _individualTimer = null;
        }

        private void Execute(object? state)
        {
            if (_cancelled) return;

            if (_labelRef.TryGetTarget(out var animatedLabel))
            {
                if (animatedLabel.BasePane == null)
                {
                    animatedLabel.StopAnimation();
                }
                else
                {
                    animatedLabel.NextFrame();
                }
            }
            else
            {
                Cancel();
                CanCloseTimer();
            }
        }
    }
}