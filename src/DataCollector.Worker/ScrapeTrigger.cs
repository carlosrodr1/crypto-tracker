using System.Threading.Channels;

namespace DataCollector.Worker;

public sealed class ScrapeTrigger
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(1);

    public void Request() => _channel.Writer.TryWrite(true);

    public ChannelReader<bool> Reader => _channel.Reader;
}
