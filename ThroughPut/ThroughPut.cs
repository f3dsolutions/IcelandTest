using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RateMonitoring
{
    public interface IInterval
    {
        DateTime From { get; }
        DateTime To { get; }
        uint LowerBoundary { get; }
        uint UpperBoundary { get; }
        uint TotalThroughPut { get;}

        List<(DateTime dateOfThroughPut, uint AmountOfThroughPut)> RecordedThroughPut { get; }

        bool UpperBoundaryExceeded { get; }
        bool LowerBoundaryNotMet { get; }
    }
    public interface IThroughPut
    {
        void RecordThroughPut(uint amountOfThroughPut, DateTime dateOfThroughPut);
        event EventHandler OnUpperBoundaryExceeded;
        event EventHandler OnLowerBoundaryNotMet;
        List<IInterval> Intervals { get; }
        bool CurrentlyExceedingUpperBoundary { get; }
        bool CurrentlyMeetingLowerBoundary { get; }
    }
    //Helps make the ThroughPut class testable
    public interface IDateTime
    {
        DateTime Now();
    }
    public class ThroughPut : IThroughPut
    {
        private bool IsCurrentlyExceedingUpperBoundary()
        {
            var currentInternal = Intervals.FirstOrDefault(x => DateTime.Now() > x.From && DateTime.Now() <= x.To);
            return currentInternal != null && currentInternal.UpperBoundaryExceeded;
        }

        private bool IsCurrentlyMeetingLowerBoundary()
        {
            var currentInternal = Intervals.FirstOrDefault(x => DateTime.Now() > x.From && DateTime.Now() <= x.To);
            return currentInternal != null && !currentInternal.LowerBoundaryNotMet;
        }
        public List<IInterval> Intervals { get; }
        private IDateTime DateTime { get; }

        public bool CurrentlyExceedingUpperBoundary => IsCurrentlyExceedingUpperBoundary();

        public bool CurrentlyMeetingLowerBoundary => IsCurrentlyMeetingLowerBoundary();

        public event EventHandler OnUpperBoundaryExceeded;
        public event EventHandler OnLowerBoundaryNotMet;

        public ThroughPut(List<IInterval> intervals, IDateTime dateTime, int tickInterval = 1800000)
        {
            Intervals = intervals;
            DateTime = dateTime;
            var timer = new System.Timers.Timer {Interval = tickInterval};
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var firstInterval = Intervals.First();
            Intervals.Remove(firstInterval);
            Intervals.Add(new Interval(firstInterval.From.AddDays(1), firstInterval.To.AddDays(1),
                firstInterval.LowerBoundary, firstInterval.UpperBoundary));
        }

        public void RecordThroughPut(uint amountOfThroughPut, DateTime dateOfThroughPut)
        {
            var existing = Intervals.FirstOrDefault(x => dateOfThroughPut > x.From && dateOfThroughPut <= x.To);
            if (existing == null)
            {
                throw new ArgumentNullException(nameof(existing));
            }
            existing.RecordedThroughPut.Add((dateOfThroughPut, amountOfThroughPut));

            if (CurrentlyExceedingUpperBoundary)
            {
                var handler = OnUpperBoundaryExceeded;
                handler?.Invoke(this, EventArgs.Empty);
            }
            if (!CurrentlyMeetingLowerBoundary)
            {
                var handler = OnLowerBoundaryNotMet;
                handler?.Invoke(this, EventArgs.Empty);
            }
            
        }
        
    }

    public class Interval : IInterval
    {
        public Interval(DateTime from, DateTime to, uint lowerBoundary, uint upperBoundary)
        {
            From = from;
            To = to;
            LowerBoundary = lowerBoundary;
            UpperBoundary = upperBoundary;
            RecordedThroughPut = new List<(DateTime dateOfThroughPut, uint AmountOfThroughPut)>();
        }
        public DateTime From { get; }
        public DateTime To { get; }
        public uint LowerBoundary { get; }
        public uint UpperBoundary { get; }
        public uint TotalThroughPut
        {
            get { return (uint) RecordedThroughPut.Sum(x => x.AmountOfThroughPut); }
        }
        public List<(DateTime dateOfThroughPut, uint AmountOfThroughPut)> RecordedThroughPut { get; }

        public bool UpperBoundaryExceeded => TotalThroughPut > UpperBoundary;

        public bool LowerBoundaryNotMet => TotalThroughPut < LowerBoundary;
    }
}
