using System.IO.Ports;
using Hsp.Midi.Messages;

namespace Zulweb.MidiPipes.Inputs;

internal class SerialInputPort : IInputPort, IDisposable
{
  private readonly SerialPort _serialPort = new();
  private CancellationTokenSource? _cts;

  private readonly byte[] _buffer = new byte[4];


  public string PortName { get; }

  public int BaudRate { get; }


  public event EventHandler<IMidiMessage>? MessageReceived;


  public SerialInputPort(string portName, int baudRate = 115200)
  {
    PortName = portName;
    BaudRate = baudRate;
  }


  public async Task Connect()
  {
    _serialPort.DtrEnable = true;
    _serialPort.BaudRate = BaudRate;
    _serialPort.PortName = PortName;

    _cts = new CancellationTokenSource();

    _ = Task.Run(() =>
    {
      var token = _cts.Token;
      var buffer = new byte[4];
      while (!token.IsCancellationRequested)
      {
        if (_serialPort is not { IsOpen: true, BytesToRead: >= 4 }) continue;
        _serialPort.Read(buffer, 0, buffer.Length);
        var message = new ChannelMessage(BitConverter.ToInt32(buffer, 0));
        Console.WriteLine(message);
      }
    });

    _serialPort.Open();
    await Task.CompletedTask;
  }

  public async Task Disconnect()
  {
    if (_cts != null)
    {
      await _cts.CancelAsync();
      _cts = null;
    }

    _serialPort.Close();
    await Task.CompletedTask;
  }


  public void Dispose()
  {
    _serialPort.Dispose();
    _cts?.Dispose();
  }
}