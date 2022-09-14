using Moq;
using Secretary.Telegram.Wrappers;

namespace Secretary.Telegram.Tests.Wrappers;

public class TelegramClientWrapperTests
{
    private Mock<ITelegramClient> _telegramClient = null!;
    private TelegramClientWrapper _wrapper = null!;

    [SetUp]
    public void Setup()
    {
        _telegramClient = new Mock<ITelegramClient>();
        _wrapper = new TelegramClientWrapper(_telegramClient.Object, 2517);
    }

    [Test]
    public async Task ShouldSendMessage()
    {
        await _wrapper.SendMessage("message");
        
        _telegramClient.Verify(target => target.SendMessage(2517, "message"));
    }

    [Test]
    public async Task ShouldSendMessageWithChoiсes()
    {
        await _wrapper.SendMessageWithKeyBoard("message", new [] { "one", "two" });
        
        _telegramClient.Verify(target => target.SendMessageWithKeyBoard(2517, "message", new [] { "one", "two" }));
    }

    [Test]
    public async Task ShouldSendDocument()
    {
        await _wrapper.SendDocument("path", "filename");
        
        _telegramClient.Verify(target => target.SendDocument(2517, "path", "filename"));
    }

    [Test]
    public async Task ShouldSendSticker()
    {
        await _wrapper.SendSticker(Stickers.Guliki);
        
        _telegramClient.Verify(target => target.SendSticker(2517, Stickers.Guliki));
    }
}