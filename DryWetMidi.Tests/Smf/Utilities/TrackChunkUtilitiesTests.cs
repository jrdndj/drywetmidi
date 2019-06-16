﻿using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestFixture]
    public sealed class TrackChunkUtilitiesTests
    {
        #region Test methods

        #region GetChannels

        [Test]
        public void GetChannels_SingleTrackChunk_NoEvents()
        {
            var trackChunk = new TrackChunk();
            CollectionAssert.IsEmpty(trackChunk.GetChannels(), "Channels collection is not empty for empty track chunk.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_NoChannelEvents()
        {
            var trackChunk = new TrackChunk(new TextEvent("Text"));
            CollectionAssert.IsEmpty(trackChunk.GetChannels(), "Channels collection is not empty for track chunk without channel events.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_SingleChannel()
        {
            var trackChunk = new TrackChunk(
                new TextEvent("Text"),
                new NoteOnEvent { Channel = (FourBitNumber)10 });
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10 },
                trackChunk.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_MultipleChannels()
        {
            var trackChunk = new TrackChunk(
                new TextEvent("Text"),
                new NoteOnEvent { Channel = (FourBitNumber)10 },
                new NoteOffEvent { Channel = (FourBitNumber)10 },
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)1 });
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10, (FourBitNumber)0, (FourBitNumber)1 },
                trackChunk.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoTrackChunks()
        {
            var trackChunks = Enumerable.Empty<TrackChunk>();
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for empty track chunks collection.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoEvents()
        {
            var trackChunks = new[] { new TrackChunk() };
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for empty track chunks.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoChannelEvents()
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(new TextEvent("Text"))
            };
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for track chunks without channel events.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_SingleChannel()
        {
            var trackChunks = new[]
            {
                new TrackChunk(new TextEvent("Text"), new NoteOnEvent { Channel = (FourBitNumber)10 }),
                new TrackChunk(new NoteOffEvent { Channel = (FourBitNumber)10 })
            };
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10 },
                trackChunks.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_MultipleChannels()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new TextEvent("Text"),
                    new NoteOnEvent { Channel = (FourBitNumber)10 },
                    new NoteOffEvent { Channel = (FourBitNumber)10 },
                    new NoteOnEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)1 }),
                new TrackChunk(
                    new TextEvent("Text 2"),
                    new NoteOnEvent { Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)2 })
            };
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10, (FourBitNumber)0, (FourBitNumber)1, (FourBitNumber)2 },
                trackChunks.GetChannels(),
                "Channels collection is invalid.");
        }

        #endregion

        #region TrimEnd

        [Test]
        public void TrimEnd_SingleTrackChunk_Empty()
        {
            var trackChunk = new TrackChunk();

            trackChunk.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(trackChunk.Events, "Trimmed empty track chunk is not empty.");
        }

        [Test]
        public void TrimEnd_SingleTrackChunk_AllMatched()
        {
            var trackChunk = new TrackChunk(new NoteOnEvent(), new NoteOffEvent(), new TextEvent());

            trackChunk.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(trackChunk.Events, "Fully trimmed track chunk is not empty.");
        }

        [Test]
        public void TrimEnd_SingleTrackChunk_NoneMatched()
        {
            var midiEvents = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent() };
            var trackChunk = new TrackChunk(midiEvents);

            trackChunk.TrimEnd(_ => false);

            CollectionAssert.AreEqual(midiEvents, trackChunk.Events, "Fully untrimmed track chunk contains different events.");
        }

        [Test]
        public void TrimEnd_SingleTrackChunk()
        {
            var midiEvents = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent() };
            var trackChunk = new TrackChunk(midiEvents);

            trackChunk.TrimEnd(e => e is TextEvent);

            CollectionAssert.AreEqual(midiEvents.Take(2).ToArray(), trackChunk.Events, "Track chunk is trimmed incorrectly.");
        }

        [Test]
        public void TrimEnd_MultipleTrackChunks_Empty()
        {
            var trackChunks = new[] { new TrackChunk(), new TrackChunk() };

            trackChunks.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(trackChunks.SelectMany(c => c.Events), "Trimmed empty track chunks are not empty.");
        }

        [Test]
        public void TrimEnd_MultipleTrackChunks_AllMatched()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent(), new NoteOffEvent(), new TextEvent());
            var trackChunk2 = new TrackChunk(new NoteOnEvent(), new NoteOffEvent(), new TextEvent());

            new[] { trackChunk1, trackChunk2 }.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(trackChunk1.Events, "Fully trimmed first track chunk is not empty.");
            CollectionAssert.IsEmpty(trackChunk2.Events, "Fully trimmed second track chunk is not empty.");
        }

        [Test]
        public void TrimEnd_MultipleTrackChunks_NoneMatched()
        {
            var midiEvents1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent() };
            var trackChunk1 = new TrackChunk(midiEvents1);
            var midiEvents2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent(), new NoteOnEvent() };
            var trackChunk2 = new TrackChunk(midiEvents2);

            new[] { trackChunk1, trackChunk2 }.TrimEnd(_ => false);

            CollectionAssert.AreEqual(midiEvents1, trackChunk1.Events, "Fully untrimmed first track chunk contains different events.");
            CollectionAssert.AreEqual(midiEvents2, trackChunk2.Events, "Fully untrimmed second track chunk contains different events.");
        }

        [Test]
        public void TrimEnd_MultipleTrackChunks()
        {
            var midiEvents1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 15 }, new TextEvent() };
            var trackChunk1 = new TrackChunk(midiEvents1);
            var midiEvents2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent { DeltaTime = 10 } };
            var trackChunk2 = new TrackChunk(midiEvents2);

            new[] { trackChunk1, trackChunk2 }.TrimEnd(e => e is TextEvent);

            CollectionAssert.AreEqual(midiEvents1.Take(2).ToArray(), trackChunk1.Events, "First track chunk is trimmed incorrectly.");
            CollectionAssert.AreEqual(midiEvents2, trackChunk2.Events, "Second track chunk is trimmed incorrectly.");
        }

        #endregion

        #endregion
    }
}
