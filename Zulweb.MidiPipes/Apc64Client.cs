using System.Text;
using System.Text.Json;
using Hsp.Midi;
using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes;

public class Apc64Client
{
  private readonly TimeSpan _messageDelay = TimeSpan.FromMilliseconds(100);
  private readonly string _baseUri;

  public Apc64Client(string baseUri)
  {
    _baseUri = baseUri;
  }

  public async Task ToggleRecordQuantize()
  {
    await Shift(true);
    await Task.Delay(_messageDelay);
    await Quantize(true);
    await Task.Delay(_messageDelay);
    await Quantize(false);
    await Task.Delay(_messageDelay);
    await Shift(false);
  }

  public async Task ToggleOverdub()
  {
    await Shift(true);
    await Task.Delay(_messageDelay);
    await Record(true);
    await Task.Delay(_messageDelay);
    await Record(false);
    await Task.Delay(_messageDelay);
    await Shift(false);
  }

  public async Task ToggleFixedLength()
  {
    await FixedLength(true);
    await Task.Delay(_messageDelay);
    await FixedLength(false);
  }

  public async Task DecreaseFixedLength()
  {
    await FixedLength(true);
    await Task.Delay(_messageDelay * 10);
    await RotateDown();
    await Task.Delay(_messageDelay);
    await FixedLength(false);
  }

  public async Task IncreaseFixedLength()
  {
    await FixedLength(true);
    await Task.Delay(_messageDelay * 10);
    await RotateUp();
    await Task.Delay(_messageDelay);
    await FixedLength(false);
  }

  private async Task SendAsync(ChannelMessage message)
  {
    var uri = new Uri(_baseUri);
    var client = new HttpClient();
    var content = new StringContent(JsonSerializer.Serialize(new
    {
      message = message.Message
    }), Encoding.UTF8, "application/json");
    await client.PostAsync(uri, content);
  }

  public async Task Shift(bool on)
  {
    await SendAsync(new ChannelMessage(
      on ? ChannelCommand.NoteOn : ChannelCommand.NoteOff,
      0,
      120,
      on ? 127 : 0
    ));
  }

  public async Task RotateUp()
  {
    await SendAsync(new ChannelMessage(
      ChannelCommand.Controller,
      0,
      90,
      1
    ));
  }

  public async Task RotateDown()
  {
    await SendAsync(new ChannelMessage(
      ChannelCommand.Controller,
      0,
      90,
      127
    ));
  }

  public async Task FixedLength(bool on)
  {
    await SendAsync(new ChannelMessage(
      on ? ChannelCommand.NoteOn : ChannelCommand.NoteOff,
      0,
      76,
      on ? 127 : 0
    ));
  }

  public async Task Quantize(bool on)
  {
    await SendAsync(new ChannelMessage(
      on ? ChannelCommand.NoteOn : ChannelCommand.NoteOff,
      0,
      75,
      on ? 127 : 0
    ));
  }

  public async Task Record(bool on)
  {
    await SendAsync(new ChannelMessage(
      on ? ChannelCommand.NoteOn : ChannelCommand.NoteOff,
      0,
      92,
      on ? 127 : 0
    ));
  }
}