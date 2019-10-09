using System;

namespace Framework_csharp.Utilities
{
    public class RetryHelper
    {
        /// <summary>
        /// Retries same action for 3 times during 3 seconds
        /// </summary>
        /// <param name="action"></param>
        /// <param name="tries"></param>
        /// <param name="pause"></param>
        public static void DoWithRetry(Action action, int tries = 3, int pause = 1000)
        {
            while (tries > 0)
            {
                try
                {
                    action();
                    return;
                }
                catch
                {
                    if (tries <= 0)
                    {
                        throw;
                    }
                    tries--;
                }
            }
        }
    }
}
