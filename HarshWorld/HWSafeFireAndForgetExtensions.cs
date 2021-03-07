using System;
using System.Threading.Tasks;

namespace HarshWorld
{
    public static class SafeFireAndForgetExtensions
    {
        public static async void SafeFireAndForget(this Task task, bool continueOnCapturedContext = true, Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }
    }
}
