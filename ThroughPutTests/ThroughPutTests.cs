using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RateMonitoring;
using Xunit;

namespace ThroughPutTests
{
    public class ThroughPutTests
    {
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsBelowConfiguredBoundaryAndNotInAndNotCurrent_ThenALowerBoundaryNotMetEventIsFired()
        {
            string eventFired = null;

            var givenConfiguredBoundaries = BuildConfiguredBoundaries();

            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());

            throughPut.OnLowerBoundaryNotMet += delegate (object sender, EventArgs e)
            {
                eventFired = e.ToString();
            };

            throughPut.RecordThroughPut(1, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.NotNull(eventFired);
        }
        
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsBelowConfiguredBoundaryAndNotInAndNotCurrent_ThenCurrentlyMeetingLowerBoundaryIsFalse()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(1, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.False(throughPut.CurrentlyMeetingLowerBoundary);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsBelowConfiguredBoundaryAndNotInAndNotCurrent_ThenExceedingUpperBoundaryIsFalse()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(1, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.False(throughPut.CurrentlyExceedingUpperBoundary);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsWithinLimits_NoEventFired()
        {
            string eventFired = null;
            string upperEventFired = null;
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();

            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());

            throughPut.OnLowerBoundaryNotMet += delegate (object sender, EventArgs e)
            {
                eventFired = e.ToString();
            };
            throughPut.OnUpperBoundaryExceeded += delegate (object sender, EventArgs e)
            {
                upperEventFired = e.ToString();
            };
            throughPut.RecordThroughPut(500, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.Null(eventFired);
            Assert.Null(upperEventFired);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsWithinLimits_ExceedingUpperBoundaryIsFalse()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(500, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.False(throughPut.CurrentlyExceedingUpperBoundary);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsWithinLimits_MeetingLowerBoundaryIsTrue()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(500, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.True(throughPut.CurrentlyMeetingLowerBoundary);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsAboveLimit_EventFired()
        {
            string eventFired = null;
            string upperEventFired = null;
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();

            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());

            throughPut.OnLowerBoundaryNotMet += delegate (object sender, EventArgs e)
            {
                eventFired = e.ToString();
            };
            throughPut.OnUpperBoundaryExceeded += delegate (object sender, EventArgs e)
            {
                upperEventFired = e.ToString();
            };
            throughPut.RecordThroughPut(5000, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.Null(eventFired);
            Assert.NotNull(upperEventFired);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsAboveLimit_ExceedingUpperBoundaryIsTrue()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(5000, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.True(throughPut.CurrentlyExceedingUpperBoundary);
        }
        [Fact]
        public void GivenConfiguredBoundaries_WhenThroughPutIsAboveLimit_MeetingLowerBoundaryIsTrue()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018());
            throughPut.RecordThroughPut(5000, DateTime.Parse("2018-01-18 12:05:16"));
            Assert.True(throughPut.CurrentlyMeetingLowerBoundary);
        }

        [Fact]
        public async Task TickInterval_RemovesFirstIntervalAndAddsNew()
        {
            var givenConfiguredBoundaries = BuildConfiguredBoundaries();
            var throughPut = new ThroughPut(givenConfiguredBoundaries, 
                new DateTimeWhichReturns1210on18Jan2018(), 5000);
            throughPut.RecordThroughPut(500, DateTime.Parse("2018-01-18 12:05:16"));
            var initialListItem = throughPut.Intervals.FirstOrDefault();
            var expectedInitial = new DateTime(2018, 1, 18);
            Assert.Equal(expectedInitial, initialListItem.From);
            await Task.Delay(5000);
            var newListItem = throughPut.Intervals.FirstOrDefault();
            Assert.Equal(expectedInitial.AddMinutes(30), newListItem.From);
        } 

        [Theory]
        [InlineData(10, 0, 126, 523)]
        [InlineData(0, 0, 5, 15)]
        [InlineData(23, 30, 4, 21)]
        [InlineData(17, 30, 49, 208)]
        public void TestList_HasCorrectValues(int hour, int minute, uint expectedLowerBoundary,
            uint expectedUpperBoundary)
        {
            var startDate = new DateTime(2018, 1, 18, hour, minute, 1);
            var list = BuildConfiguredBoundaries();
            var listItem = 
                list.FirstOrDefault(x => startDate > x.From && startDate <= x.To);
            Assert.NotNull(listItem);
            Assert.Equal(expectedLowerBoundary, listItem.LowerBoundary);
            Assert.Equal(expectedUpperBoundary, listItem.UpperBoundary);
        }

        public class DateTimeWhichReturns1210on18Jan2018 : IDateTime
        {
            public DateTime Now()
            {
                return DateTime.Parse("2018-01-18 12:10:00");
            }
        }

        
        List<IInterval> BuildConfiguredBoundaries()
        { 
            var startingDate = new DateTime(2018, 1, 18);
            var offset = 30;
            return new List<IInterval>()
            {
                new Interval(startingDate, startingDate.AddMinutes(offset * 1), 5, 15),
                new Interval(startingDate.AddMinutes(offset * 1), startingDate.AddMinutes(offset * 2), 6, 18),
                new Interval(startingDate.AddMinutes(offset * 2), startingDate.AddMinutes(offset * 3), 7, 21),
                new Interval(startingDate.AddMinutes(offset * 3), startingDate.AddMinutes(offset * 4), 8, 25),
                new Interval(startingDate.AddMinutes(offset * 4), startingDate.AddMinutes(offset * 5), 9, 30),
                new Interval(startingDate.AddMinutes(offset * 5), startingDate.AddMinutes(offset * 6), 10, 36),
                new Interval(startingDate.AddMinutes(offset * 6), startingDate.AddMinutes(offset * 7), 12, 43),
                new Interval(startingDate.AddMinutes(offset * 7), startingDate.AddMinutes(offset * 8), 14, 51),
                new Interval(startingDate.AddMinutes(offset * 8), startingDate.AddMinutes(offset * 9), 16, 61),
                new Interval(startingDate.AddMinutes(offset * 9), startingDate.AddMinutes(offset * 10), 19, 73),
                new Interval(startingDate.AddMinutes(offset * 10), startingDate.AddMinutes(offset * 11), 22, 87),
                new Interval(startingDate.AddMinutes(offset * 11), startingDate.AddMinutes(offset * 12), 26, 104),
                new Interval(startingDate.AddMinutes(offset * 12), startingDate.AddMinutes(offset * 13), 31, 124),
                new Interval(startingDate.AddMinutes(offset * 13), startingDate.AddMinutes(offset * 14), 37, 148),
                new Interval(startingDate.AddMinutes(offset * 14), startingDate.AddMinutes(offset * 15), 44, 177),
                new Interval(startingDate.AddMinutes(offset * 15), startingDate.AddMinutes(offset * 16), 52, 212),
                new Interval(startingDate.AddMinutes(offset * 16), startingDate.AddMinutes(offset * 17), 62, 254),
                new Interval(startingDate.AddMinutes(offset * 17), startingDate.AddMinutes(offset * 18), 74, 304),
                new Interval(startingDate.AddMinutes(offset * 18), startingDate.AddMinutes(offset * 19), 88, 364),
                new Interval(startingDate.AddMinutes(offset * 19), startingDate.AddMinutes(offset * 20), 105, 436),
                new Interval(startingDate.AddMinutes(offset * 20), startingDate.AddMinutes(offset * 21), 126, 523),
                new Interval(startingDate.AddMinutes(offset * 21), startingDate.AddMinutes(offset * 22), 151, 627),
                new Interval(startingDate.AddMinutes(offset * 22), startingDate.AddMinutes(offset * 23), 181, 752),
                new Interval(startingDate.AddMinutes(offset * 23), startingDate.AddMinutes(offset * 24), 217, 902),
                new Interval(startingDate.AddMinutes(offset * 24), startingDate.AddMinutes(offset * 25), 260, 1082),
                new Interval(startingDate.AddMinutes(offset * 25), startingDate.AddMinutes(offset * 26), 312, 1298),
                new Interval(startingDate.AddMinutes(offset * 26), startingDate.AddMinutes(offset * 27), 260, 1081),
                new Interval(startingDate.AddMinutes(offset * 27), startingDate.AddMinutes(offset * 28), 216, 900),
                new Interval(startingDate.AddMinutes(offset * 28), startingDate.AddMinutes(offset * 29), 180, 750),
                new Interval(startingDate.AddMinutes(offset * 29), startingDate.AddMinutes(offset * 30), 150, 625),
                new Interval(startingDate.AddMinutes(offset * 30), startingDate.AddMinutes(offset * 31), 125, 520),
                new Interval(startingDate.AddMinutes(offset * 31), startingDate.AddMinutes(offset * 32), 104, 433),
                new Interval(startingDate.AddMinutes(offset * 32), startingDate.AddMinutes(offset * 33), 86, 360),
                new Interval(startingDate.AddMinutes(offset * 33), startingDate.AddMinutes(offset * 34), 71, 300),
                new Interval(startingDate.AddMinutes(offset * 34), startingDate.AddMinutes(offset * 35), 59, 250),
                new Interval(startingDate.AddMinutes(offset * 35), startingDate.AddMinutes(offset * 36), 49, 208),
                new Interval(startingDate.AddMinutes(offset * 36), startingDate.AddMinutes(offset * 37), 40, 173),
                new Interval(startingDate.AddMinutes(offset * 37), startingDate.AddMinutes(offset * 38), 33, 144),
                new Interval(startingDate.AddMinutes(offset * 38), startingDate.AddMinutes(offset * 39), 27, 120),
                new Interval(startingDate.AddMinutes(offset * 39), startingDate.AddMinutes(offset * 40), 22, 100),
                new Interval(startingDate.AddMinutes(offset * 40), startingDate.AddMinutes(offset * 41), 18, 83),
                new Interval(startingDate.AddMinutes(offset * 41), startingDate.AddMinutes(offset * 42), 15, 69),
                new Interval(startingDate.AddMinutes(offset * 42), startingDate.AddMinutes(offset * 43), 12, 57),
                new Interval(startingDate.AddMinutes(offset * 43), startingDate.AddMinutes(offset * 44), 10, 47),
                new Interval(startingDate.AddMinutes(offset * 44), startingDate.AddMinutes(offset * 45), 8, 39),
                new Interval(startingDate.AddMinutes(offset * 45), startingDate.AddMinutes(offset * 46), 6, 32),
                new Interval(startingDate.AddMinutes(offset * 46), startingDate.AddMinutes(offset * 47), 5, 26),
                new Interval(startingDate.AddMinutes(offset * 47), startingDate.AddMinutes(offset * 48), 4, 21),
            };
        }
    }
}
