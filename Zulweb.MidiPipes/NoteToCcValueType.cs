namespace Zulweb.MidiPipes;

public enum NoteToCcValueType
{
  /// <summary>
  /// Use the MIDI note number as the controller value.
  /// </summary>
  NoteNumber,
  /// <summary>
  /// Use the MIDI note velocity as the controller value.
  /// </summary>
  Velocity
}