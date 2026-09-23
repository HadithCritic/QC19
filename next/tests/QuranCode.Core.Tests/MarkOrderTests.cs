using System.Text;
using QuranCode.Core.Text;
using Xunit;

namespace QuranCode.Core.Tests;

public sealed class MarkOrderTests
{
    [Fact]
    public void PutsFathaBeforeShadda()
    {
        string source = "\u0671\u0644\u0644\u0651\u064E\u0647\u0650"; // as stored: shadda, then fatha
        string typed = "\u0671\u0644\u0644\u064E\u0651\u0647\u0650";
        Assert.Equal(typed, MarkOrder.Canonical(source));
        Assert.Same(typed, MarkOrder.Canonical(typed));
    }

    [Fact]
    public void ReordersEveryWordCanonically()
    {
        using var engine = new QuranCodeEngine(TestPaths.SubmissionDatabase);
        var wrong = new List<string>();
        foreach (Content.Verse verse in engine.Verses)
        {
            foreach (string word in verse.Text.Split(' '))
            {
                string reordered = MarkOrder.Canonical(word);

                // Same text to Unicode (this test runs with full globalization)...
                bool equivalent = reordered.Normalize(NormalizationForm.FormD) == word.Normalize(NormalizationForm.FormD);

                // ...with each run of marks in nondecreasing combining class.
                bool ordered = true;
                for (int i = 1; i < reordered.Length; i++)
                {
                    int cls = MarkOrder.CombiningClass(reordered[i]);
                    if (cls != 0 && MarkOrder.CombiningClass(reordered[i - 1]) > cls) ordered = false;
                }
                if (!equivalent || !ordered) wrong.Add(word);
            }
        }
        Assert.Empty(wrong.Distinct().Take(10));
    }
}
