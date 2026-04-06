namespace Tips_Player.Views.Controls;

public partial class VerticalSlider : ContentView
{
    // ── Bindable Properties ───────────────────────────────────────────────────

    public static readonly BindableProperty MinimumProperty =
        BindableProperty.Create(nameof(Minimum), typeof(double), typeof(VerticalSlider), -12.0,
            propertyChanged: (b, _, _) => ((VerticalSlider)b).UpdateVisuals());

    public static readonly BindableProperty MaximumProperty =
        BindableProperty.Create(nameof(Maximum), typeof(double), typeof(VerticalSlider), 12.0,
            propertyChanged: (b, _, _) => ((VerticalSlider)b).UpdateVisuals());

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(VerticalSlider), 0.0,
            BindingMode.TwoWay,
            propertyChanged: (b, _, _) => ((VerticalSlider)b).UpdateVisuals());

    public static readonly BindableProperty ThumbColorProperty =
        BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(VerticalSlider),
            Color.FromArgb("#6366F1"));

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(VerticalSlider),
            Color.FromArgb("#3A3A3A"),
            propertyChanged: (b, _, n) => ((VerticalSlider)b).TrackBg.Color = (Color)n);

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, Math.Clamp(value, Minimum, Maximum));
    }

    public Color ThumbColor
    {
        get => (Color)GetValue(ThumbColorProperty);
        set => SetValue(ThumbColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    // ── State ─────────────────────────────────────────────────────────────────

    private double _valueAtPanStart;
    private double _trackHeight;   // cached on SizeChanged

    public VerticalSlider()
    {
        InitializeComponent();
        SizeChanged += (_, _) =>
        {
            _trackHeight = Height;
            UpdateVisuals();
        };
    }

    // ── Gesture Handlers ──────────────────────────────────────────────────────

    private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        double range = Maximum - Minimum;
        if (range <= 0 || _trackHeight <= 0) return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _valueAtPanStart = Value;
                break;

            case GestureStatus.Running:
                // TotalY is positive downward → dragging down decreases value
                double delta = -e.TotalY / _trackHeight * range;
                Value = Math.Clamp(_valueAtPanStart + delta, Minimum, Maximum);
                break;
        }
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (_trackHeight <= 0) return;
        var pos = e.GetPosition(this);
        if (pos == null) return;

        // Map tap Y to value: top = Maximum, bottom = Minimum
        double ratio = 1.0 - Math.Clamp(pos.Value.Y / _trackHeight, 0, 1);
        Value = Minimum + ratio * (Maximum - Minimum);
    }

    // ── Visual Update ─────────────────────────────────────────────────────────

    private void UpdateVisuals()
    {
        double range = Maximum - Minimum;
        if (range <= 0 || _trackHeight <= 0) return;

        // Fraction from 0 (Minimum) to 1 (Maximum)
        double fraction = Math.Clamp((Value - Minimum) / range, 0, 1);

        double fillHeight  = fraction * _trackHeight;
        double thumbOffset = (1.0 - fraction) * _trackHeight;   // distance from top

        TrackFill.HeightRequest = fillHeight;

        // Shift thumb: VerticalOptions=End means it sits at the bottom; use Margin to lift it
        Thumb.Margin = new Thickness(0, 0, 0, fillHeight - 9); // 9 = half thumb size
        TrackFill.Margin = new Thickness(0, 0, 0, 0);

        // Also update fill colour to match current thumb colour
        TrackFill.Color = ThumbColor;
    }
}
