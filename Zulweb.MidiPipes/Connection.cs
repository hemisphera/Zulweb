﻿using System.Text.Json.Serialization;
using Hsp.Midi.Messages;
using Microsoft.Extensions.Logging;
using Zulweb.MidiPipes.Chains;
using Zulweb.MidiPipes.Inputs;

namespace Zulweb.MidiPipes;

public class Connection
{
  private string _name = string.Empty;
  private ILogger? _logger;

  [JsonIgnore]
  public bool Connected { get; private set; }

  /// <summary>
  /// Specifies if the connection is enabled. Default is true.
  /// Disabled connections will not be connected.
  /// </summary>
  public bool Enabled { get; set; } = true;

  /// <summary>
  /// An optional name for the connection.
  /// If no name is given, the input port name will be used.
  /// </summary>
  public string Name
  {
    get => string.IsNullOrEmpty(_name) ? InputPort : _name;
    set => _name = value;
  }

  /// <summary>
  /// An optional description for the connection.
  /// </summary>
  public string? Description { get; set; }

  /// <summary>
  /// The input port type to connect to.
  /// </summary>
  public InputPortType Type { get; set; }

  /// <summary>
  /// The name of the input port to connect to.
  /// </summary>
  public string InputPort { get; set; } = string.Empty;

  /// <summary>
  /// The default output port, if not specified in the chain items.
  /// </summary>
  public string? DefaultOutputPort { get; set; }

  /// <summary>
  /// The chain of items to process the incoming messages.
  /// </summary>
  public MidiChain[]? Chains { get; set; }

  public IInputPort? Port { get; private set; }


  public async Task<bool> TryConnect(ILoggerFactory? loggerFactory)
  {
    try
    {
      await Connect(loggerFactory);
      return true;
    }
    catch (Exception e)
    {
      _logger?.LogError(e, "Failed to connect connection {name}.", Name);
      return false;
    }
  }

  public async Task Connect(ILoggerFactory? loggerFactory)
  {
    if (!Enabled) return;
    if (Connected) return;

    _logger = loggerFactory?.CreateLogger<Connection>();

    Port = CreatePort();

    try
    {
      await Port.Connect();
      Port.MessageReceived += DeviceOnMessageReceived;
      await Task.WhenAll((Chains ?? []).Select(chain => chain.Initialize(this, loggerFactory)));
      _logger?.LogInformation("Connected: {name}", Name);
      Connected = true;
    }
    catch
    {
      await Disconnect();
      throw;
    }
  }

  private IInputPort CreatePort()
  {
    return Type switch
    {
      InputPortType.Midi => new MidiInputPort(InputPort, _logger),
      InputPortType.Serial => new SerialInputPort(InputPort),
      InputPortType.WebRequest => new WebRequestInputPort(InputPort),
      _ => throw new ArgumentOutOfRangeException()
    };
  }

  public async Task Disconnect()
  {
    if (!Enabled) return;
    await Task.WhenAll((Chains ?? []).Select(chain => chain.Deinitialize()));

    if (Port != null)
    {
      Port.MessageReceived -= DeviceOnMessageReceived;
      await Port.Disconnect();
      if (Port is IAsyncDisposable iadp) await iadp.DisposeAsync();
      else if (Port is IDisposable idp) idp.Dispose();
      Port = null;
    }

    Connected = false;
  }


  private void DeviceOnMessageReceived(object? sender, IMidiMessage e)
  {
    if (_logger?.IsEnabled(LogLevel.Debug) == true)
    {
      _logger.LogDebug("Received on {port}: {message}", InputPort, e);
    }

    foreach (var chain in Chains ?? [])
    {
      if (!chain.Enabled) continue;
      Dispatch(e, chain);
    }
  }

  public void Dispatch(IMidiMessage midiMessage, MidiChain chain)
  {
    var cr = new ChainRunner(this, chain.Items ?? []);
    _ = cr.Run(midiMessage);
  }
}