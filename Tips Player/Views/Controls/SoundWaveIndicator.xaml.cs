namespace Tips_Player.Views.Controls;

public partial class SoundWaveIndicator : ContentView
{
    // ── Bindable properties ────────────────────────────────────────────────────

    public static readonly BindableProperty IsPlayingProperty =
        BindableProperty.Create(nameof(IsPlaying), typeof(bool), typeof(SoundWaveIndicator), false,
            propertyChanged: (b, _, n) => ((SoundWaveIndicator)b).OnIsPlayingChanged((bool)n));

    public static readonly BindableProperty BarColorProperty =
        BindableProperty.Create(nameof(BarColor), typeof(Color), typeof(SoundWaveIndicator),
            Color.FromArgb("#6366F1"),
            propertyChanged: (b, _, n) => ((SoundWaveIndicator)b).ApplyBarColor((Color)n));

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set => SetValue(BarColorProperty, value);
    }

    // ── Two animation key-frames (ScaleY values per bar) ──────────────────────

    private static readonly double[] FrameA = [0.25, 0.90, 0.50, 1.00];
    private static readonly double[] FrameB = [1.00, 0.35, 0.85, 0.45];
    private static readonly double[] Paused = [0.40, 0.65, 0.50, 0.70];
    private static readonly uint[]   Delays = [280,  360,  320,  300 ];

    private CancellationTokenSource? _cts;

    // ── Init ──────────────────────────────────────────────────────────────────

    public SoundWaveIndicator()
    {
        InitializeComponent();
        ApplyBarColor(BarColor);
        SetScaleY(Paused);
    }

    // ── Property handlers ─────────────────────────────────────────────────────

    private void OnIsPlayingChanged(bool playing)
    {
        if (playing) StartAnimation();
        else StopAnimation();
    }

    private void ApplyBarColor(Color color)
    {
        Bar1.BackgroundColor = color;
        Bar2.BackgroundColor = color;
        Bar3.BackgroundColor = color;
        Bar4.BackgroundColor = color;
    }

    // ── Animation ─────────────────────────────────────────────────────────────

    private void StartAnimation()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            int frame = 0;
            while (!token.IsCancellationRequested)
            {
                double[] target = frame % 2 == 0 ? FrameA : FrameB;

                var t1 = Bar1.ScaleYToAsync(target[0], Delays[0], Easing.SinInOut);
                var t2 = Bar2.ScaleYToAsync(target[1], Delays[1], Easing.SinInOut);
                var t3 = Bar3.ScaleYToAsync(target[2], Delays[2], Easing.SinInOut);
                var t4 = Bar4.ScaleYToAsync(target[3], Delays[3], Easing.SinInOut);
                await Task.WhenAll(t1, t2, t3, t4);

                frame++;

                if (token.IsCancellationRequested) break;
                await Task.Delay(40, CancellationToken.None);
            }
        });
    }

    private void StopAnimation()
    {
        _cts?.Cancel();
        _cts = null;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.WhenAll(
                Bar1.ScaleYToAsync(Paused[0], 200, Easing.CubicOut),
                Bar2.ScaleYToAsync(Paused[1], 200, Easing.CubicOut),
                Bar3.ScaleYToAsync(Paused[2], 200, Easing.CubicOut),
                Bar4.ScaleYToAsync(Paused[3], 200, Easing.CubicOut));
        });
    }

    private void SetScaleY(double[] values)
    {
        Bar1.ScaleY = values[0];
        Bar2.ScaleY = values[1];
        Bar3.ScaleY = values[2];
        Bar4.ScaleY = values[3];
    }
}
