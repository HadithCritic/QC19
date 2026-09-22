using System;
using System.Collections.Generic;

namespace Model
{
    public struct NumberQuery
    {
        /// <summary>
        /// Number of word/verse/chapter or sum of numbers of a Ws/Vs/Cs range/set
        /// </summary>
        public int Number;
        public NumberScope NumberScope;
        public int PartitionCount;
        public int VerseCount;
        public int WordCount;
        public int LetterCount;
        public int UniqueLetterCount;
        public long Value;
        public int Frequency;
        public int Occurrence;

        public NumberType NumberNumberType;
        public NumberType PartitionCountNumberType;
        public NumberType VerseCountNumberType;
        public NumberType WordCountNumberType;
        public NumberType LetterCountNumberType;
        public NumberType UniqueLetterCountNumberType;
        public NumberType ValueNumberType;
        public NumberType FrequencyNumberType;
        public NumberType OccurrenceNumberType;

        public ComparisonOperator NumberComparisonOperator;
        public ComparisonOperator PartitionCountComparisonOperator;
        public ComparisonOperator VerseCountComparisonOperator;
        public ComparisonOperator WordCountComparisonOperator;
        public ComparisonOperator LetterCountComparisonOperator;
        public ComparisonOperator UniqueLetterCountComparisonOperator;
        public ComparisonOperator ValueComparisonOperator;
        public ComparisonOperator FrequencyComparisonOperator;
        public ComparisonOperator OccurrenceComparisonOperator;
        public int NumberRemainder;
        public int PartitionCountRemainder;
        public int VerseCountRemainder;
        public int WordCountRemainder;
        public int LetterCountRemainder;
        public int UniqueLetterCountRemainder;
        public int ValueRemainder;
        public int FrequencyRemainder;
        public int OccurrenceRemainder;

        public bool IsValid(NumbersResultType numbers_result_type)
        {
            switch (numbers_result_type)
            {
                case NumbersResultType.Letters:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            IsValidLetterSearch()
                        );
                    }
                case NumbersResultType.LetterRanges:
                case NumbersResultType.LetterSets:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            (
                            (LetterCount != 1)
                            &&
                            IsValidLetterSearch()
                            )
                        );
                    }
                case NumbersResultType.Words:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            IsValidWordSearch()
                        );
                    }
                case NumbersResultType.WordRanges:
                case NumbersResultType.WordSets:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            (
                            (WordCount != 1)
                            &&
                            IsValidWordSearch()
                            )
                        );
                    }
                case NumbersResultType.Sentences:
                    {
                        return
                        (
                            IsValidVerseSearch()
                        );
                    }
                case NumbersResultType.Verses:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            IsValidVerseSearch()
                        );
                    }
                case NumbersResultType.VerseRanges:
                case NumbersResultType.VerseSets:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            (
                            (VerseCount != 1)
                            &&
                            IsValidVerseSearch()
                            )
                        );
                    }
                case NumbersResultType.Chapters:
                case NumbersResultType.Pages:
                case NumbersResultType.Stations:
                case NumbersResultType.Parts:
                case NumbersResultType.Groups:
                case NumbersResultType.Halfs:
                case NumbersResultType.Quarters:
                case NumbersResultType.Bowings:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            IsValidPartitionSearch()
                        );
                    }
                case NumbersResultType.ChapterRanges:
                case NumbersResultType.PageRanges:
                case NumbersResultType.StationRanges:
                case NumbersResultType.PartRanges:
                case NumbersResultType.GroupRanges:
                case NumbersResultType.HalfRanges:
                case NumbersResultType.QuarterRanges:
                case NumbersResultType.BowingRanges:
                case NumbersResultType.ChapterSets:
                case NumbersResultType.PageSets:
                case NumbersResultType.StationSets:
                case NumbersResultType.PartSets:
                case NumbersResultType.GroupSets:
                case NumbersResultType.HalfSets:
                case NumbersResultType.QuarterSets:
                case NumbersResultType.BowingSets:
                    {
                        return
                        (
                            IsValidNumberSearch()
                            ||
                            (
                            (PartitionCount != 1)
                            &&
                            IsValidPartitionSearch()
                            )
                        );
                    }
                default:
                    {
                        return false;
                    }
            }
        }
        // helper methods
        private bool IsValidNumberSearch()
        {
            return (
                    (Number != 0) ||
                    (NumberNumberType != NumberType.None)
                   );
        }
        private bool IsValidPartitionSearch()
        {
            return (
                    (VerseCount > 0) ||
                    (VerseCountNumberType != NumberType.None) ||
                    IsValidVerseSearch()
                   );
        }
        private bool IsValidVerseSearch()
        {
            return (
                    (WordCount > 0) ||
                    (WordCountNumberType != NumberType.None) ||
                    IsValidWordSearch()
                   );
        }
        private bool IsValidWordSearch()
        {
            return (
                    (LetterCount > 0) ||
                    (UniqueLetterCount > 0) ||
                    IsValidLetterSearch()
                   );
        }
        private bool IsValidLetterSearch()
        {
            return (
                    (Value > 0) ||
                    (Frequency > 0) ||
                    (Occurrence > 0) ||
                    (LetterCountNumberType != NumberType.None) ||
                    (UniqueLetterCountNumberType != NumberType.None) ||
                    (ValueNumberType != NumberType.None) ||
                    (FrequencyNumberType != NumberType.None) ||
                    (OccurrenceNumberType != NumberType.None)
                   );
        }
    }
}
