using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    public static class ExceptionHandler
    {

        public static void LogException(Exception ex)
        {
            // e.g. MarkedUp.AnalyticClient.Error(ex.Message, ex);
        }

        /// <summary>
        /// Handles failure for application exception on UI thread (or initiated from UI thread via async void handler)
        /// </summary>
        public static void HandleException(Exception ex)
        {

            LogException(ex);
        }

        /// <summary>
        /// Gets the error message to display from an exception
        /// </summary>
        public static string GetDisplayMessage(Exception ex)
        {
            string errorMessage;
#if DEBUG
            errorMessage = (ex.Message + " " + ex.StackTrace);
#else
                errorMessage = "An unknown error has occurred, please try again";
#endif

            return errorMessage;
        }

    }
}
