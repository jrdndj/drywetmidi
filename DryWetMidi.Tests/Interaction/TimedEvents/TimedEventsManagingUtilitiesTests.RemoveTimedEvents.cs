﻿using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_EmptyCollection([Values] bool wrapToTrackChunk, [Values] bool predicateValue)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                match: e => predicateValue,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_Matched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOn,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_NotMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOff,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_AllMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_1([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[] { new NoteOffEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_2([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_3([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 }, new NoteOffEvent { DeltaTime = 150 }, new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOnEvent { DeltaTime = 350, NoteNumber = (SevenBitNumber)23 } },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_NotMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_Big_RemoveHalf([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: Enumerable.Range(0, 50000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 50000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray(),
                expectedRemovedCount: 50000);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate_Big_RemoveAll([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 100000);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_EmptyCollection([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0]);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_OneEvent([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_MultipleEvents([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() });
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_Big_RemoveAll([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray());
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_EmptyCollection([Values] bool wrapToTrackChunk, [Values] bool predicateValue)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                match: e => predicateValue,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_Matched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOn,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_NotMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                new[] { new NoteOnEvent() },
                match: e => e.Event.EventType == MidiEventType.NoteOff,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_AllMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_1([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[] { new NoteOffEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_2([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_3([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 200 }, new NoteOffEvent { DeltaTime = 150 }, new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOnEvent { DeltaTime = 350, NoteNumber = (SevenBitNumber)23 } },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_NotMatched([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_Big_RemoveHalf([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: Enumerable.Range(0, 50000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 50000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray(),
                expectedRemovedCount: 50000);
        }

        [Test]
        public void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate_Big_RemoveAll([Values] bool wrapToTrackChunk)
        {
            RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedRemovedCount: 100000);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                match: e => true,
                expectedMidiEvents: new MidiEvent[0][],
                expectedRemovedCount: 0,
                expectedTotalChunkIndices: new int[0]);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 0,
                expectedTotalChunkIndices: new int[0]);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_OneEvent_1_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1,
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_OneEvent_1_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => false,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedRemovedCount: 0,
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_OneEvent_2_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1,
                expectedTotalChunkIndices: new[] { 1 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_OneEvent_2_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => false,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                expectedRemovedCount: 0,
                expectedTotalChunkIndices: new[] { 1 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                expectedRemovedCount: 0,
                expectedTotalChunkIndices: new[] { 0, 0, 1, 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_SomeMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[0]
                },
                expectedRemovedCount: 2,
                expectedTotalChunkIndices: new[] { 0, 0, 1, 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_SomeMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2,
                expectedTotalChunkIndices: new[] { 0, 0, 1, 1 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_SomeMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2,
                expectedTotalChunkIndices: new[] { 1, 1, 1, 1, 0, 0, 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_AllMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4,
                expectedTotalChunkIndices: new[] { 0, 0, 1, 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_AllMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4,
                expectedTotalChunkIndices: new[] { 0, 0, 1, 1 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleEvents_AllMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Time >= 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0], 
                    new MidiEvent[0]
                },
                expectedRemovedCount: 7,
                expectedTotalChunkIndices: new[] { 1, 1, 1, 1, 0, 0, 0 });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_Big_RemoveHalf([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 50000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 50000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray()).ToArray(),
                expectedRemovedCount: 500000,
                expectedTotalChunkIndices: Enumerable.Range(0, 50000).SelectMany(i => Enumerable.Range(0, 10).SelectMany(j => new[] { j, j })).ToArray());
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate_Big_RemoveAll([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(i => new MidiEvent[0]).ToArray(),
                expectedRemovedCount: 1000000,
                expectedTotalChunkIndices: Enumerable.Range(0, 10).SelectMany(i => Enumerable.Range(0, 100000).Select(j => i)).ToArray());
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                match: e => true,
                expectedMidiEvents: new MidiEvent[0][],
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneEvent_1_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneEvent_1_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                match: e => false,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneEvent_2_Matched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => true,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedRemovedCount: 1);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneEvent_2_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                match: e => false,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_NotMatched([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                expectedRemovedCount: 0);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[0]
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOffEvent { DeltaTime = 10 } }
                },
                expectedRemovedCount: 2);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 4);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                },
                match: e => e.Time >= 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[0],
                    new MidiEvent[0]
                },
                expectedRemovedCount: 7);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_WithPredicate_Big_RemoveHalf([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 50000).SelectMany(i => new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent() }).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 50000).Select(i => new NoteOffEvent { DeltaTime = 100 }).ToArray()).ToArray(),
                expectedRemovedCount: 500000);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate_Big_RemoveAll([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray()).ToArray(),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: Enumerable.Range(0, 10).Select(i => new MidiEvent[0]).ToArray(),
                expectedRemovedCount: 1000000);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_EmptyTrackChunks([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneEvent_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneEvent_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleEvents_AllMatched_1([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new[] { new NoteOnEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleEvents_AllMatched_2([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleEvents_AllMatched_3([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 20 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 10 } },
                    new MidiEvent[] { new TextEvent(), new TextEvent("A"), new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent() }
                });
        }

        [Test]
        public void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_Big_RemoveAll([Values] bool wrapToFile)
        {
            RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: Enumerable.Range(0, 10).Select(j => Enumerable.Range(0, 100000).Select(i => new NoteOnEvent()).ToArray()).ToArray());
        }

        #endregion

        #region Private methods

        private void RemoveTimedEvents_EventsCollection_WithIndices_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var totalIndices = new List<int>();

            ProcessTimedEventPredicate processTimedEventPredicate = (timedEvent, iEventsCollection, iTotal) =>
            {
                Assert.AreEqual(0, iEventsCollection, "Events collection index is invalid.");
                totalIndices.Add(iTotal);
                return match(timedEvent);
            };

            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunk.RemoveTimedEvents(processTimedEventPredicate),
                    "Invalid count of removed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    expectedRemovedCount,
                    eventsCollection.RemoveTimedEvents(processTimedEventPredicate),
                    "Invalid count of removed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), totalIndices, "Invalid total indices.");
        }

        private void RemoveTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunk.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    expectedRemovedCount,
                    eventsCollection.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }
        }

        private void RemoveTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    trackChunk.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                var expectedTrackChunk = new TrackChunk();
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    eventsCollection.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                var expectedEventsCollection = new EventsCollection();
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }
        }

        private void RemoveTimedEvents_TrackChunks_WithIndices_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount,
            ICollection<int> expectedTotalChunkIndices)
        {
            var totalIndices = new List<int>();
            var totalChunkIndices = new List<int>();

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            ProcessTimedEventPredicate processTimedEventPredicate = (timedEvent, iChunk, iTotal) =>
            {
                totalIndices.Add(iTotal);
                totalChunkIndices.Add(iChunk);
                return match(timedEvent);
            };

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveTimedEvents(processTimedEventPredicate),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveTimedEvents(processTimedEventPredicate),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Sum(e => e.Count)), totalIndices, "Invalid total indices.");
            CollectionAssert.AreEqual(expectedTotalChunkIndices, totalChunkIndices, "Invalid total chunk indices.");
        }

        private void RemoveTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveTimedEvents(match),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }
        }

        private void RemoveTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(midiEvents.Select(e => new TrackChunk())), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.RemoveTimedEvents(),
                    "Invalid count of removed timed events.");

                MidiAsserts.AreEqual(midiEvents.Select(e => new TrackChunk()), trackChunks, true, "Events are invalid.");
            }
        }

        #endregion
    }
}
