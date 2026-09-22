using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuranCode.Desktop.Services;

namespace QuranCode.Desktop.ViewModels;

/// <summary>One system's value for the current text.</summary>
public sealed record SystemValue(string System, long Value, long DigitSum, long DigitalRoot);

/// <summary>
/// Value arbitrary text across value systems.
/// </summary>
/// <remarks>
/// Covers Features.txt #2, "display the values of selected text in all
/// numerical systems at once". 407 systems are installed, so the page values
/// against a chosen subset by default and can widen on request.
/// </remarks>
public sealed partial class NumerologyViewModel : FeatureViewModel
{
    public override string Title => "Numerology";
    public override string Description => "Value text across numerical systems";

    public ObservableCollection<SystemValue> Values { get; } = [];
    public ObservableCollection<string> Systems { get; } = [];

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _allSystems;

    [ObservableProperty]
    private string _summary = "Enter Arabic text and press Calculate.";

    public NumerologyViewModel(EngineService engines) : base(engines) { }

    protected override void OnActivated()
    {
        foreach (string system in Engines.Engine.ValueSystems()) Systems.Add(system);
        Summary = $"{Systems.Count} value systems installed.";
    }

    [RelayCommand]
    private async Task CalculateAsync()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            Summary = "Enter some text.";
            return;
        }

        IsBusy = true;
        Error = null;
        Values.Clear();

        try
        {
            string text = Text;
            bool all = AllSystems;

            List<SystemValue> rows = await Task.Run(() =>
            {
                var engine = Engines.Engine;

                // Valuing against all 407 systems is a few hundred passes over a
                // short string, which is fast, but it still belongs off the UI
                // thread because the first call may build a segmentation.
                IEnumerable<string> systems = all
                    ? engine.ValueSystems()
                    : engine.ValueSystems().Where(IsCommon);

                var list = new List<SystemValue>();
                foreach (string system in systems)
                {
                    long value = engine.Value(text, system);
                    list.Add(new SystemValue(
                        system,
                        value,
                        QuranCode.Core.Numbers.NumberTheory.DigitSum(value),
                        QuranCode.Core.Numbers.NumberTheory.DigitalRoot(value)));
                }
                return list;
            });

            foreach (SystemValue row in rows) Values.Add(row);
            Summary = $"{rows.Count} systems evaluated.";
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            Summary = "Calculation failed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// The systems a reader is most likely to want first: the Primalogy default
    /// and the classical orderings, rather than all 407.
    /// </summary>
    private static bool IsCommon(string name) =>
        name.EndsWith("_Alphabet_Primes1", StringComparison.Ordinal) ||
        name.EndsWith("_Abjad_Gematria", StringComparison.Ordinal);
}
