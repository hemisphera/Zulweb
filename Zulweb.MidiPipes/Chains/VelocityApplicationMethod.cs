namespace Zulweb.MidiPipes.Chains;

public enum VelocityApplicationMethod
{
  /// <summary>
  /// The output is truncated to the range.
  /// </summary>
  Limit,
  /// <summary>
  /// The output velocity is in the specified range, proportinal to the input velocity.
  /// </summary>
  Translate
}