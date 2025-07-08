namespace Zulweb.LedProxy;

public interface IPackageSender
{
  Task Send(byte[] data, CancellationToken cancellationToken);
}