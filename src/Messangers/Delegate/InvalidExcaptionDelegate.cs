namespace Messangers.Delegate
{
    public class InvalidExcaptionDelegate
    {
        private readonly ILogger<InvalidExcaptionDelegate> _logger;

        public InvalidExcaptionDelegate (ILogger<InvalidExcaptionDelegate> logger)
        {
            _logger = logger;
        }

        public async Task RunDelegate(InvalidOperationException ex)
        {
            _logger.LogError("Не валидная операция" + ex.Message + ex.StackTrace + ex.InnerException);
        }
    }
}
