using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Model
{
    using System.IO.IsolatedStorage;

    public class AuthObj
    {
        public string PP
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("PP"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["PP"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["PP"] = value;
            }
        }

        public string Me
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("ME"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["ME"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["ME"] = value;
            }
        }

        public string Id
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("ID"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["ID"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["ID"] = value;
            }
        }

        public string UserName
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("USER"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["USER"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["USER"] = value;
            }
        }

        public string Mark
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("MARK"))
                {
                    var mark = IsolatedStorageSettings.ApplicationSettings["MARK"] as string;
                    if (string.IsNullOrEmpty(mark))
                    {
                        return "1";
                    }

                    return IsolatedStorageSettings.ApplicationSettings["MARK"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["MARK"] = value;
            }
        }

        public bool IsAuthenticated { get; set; }

        public void Clean()
        {
            PP = string.Empty;
            Me = string.Empty;
            Id = string.Empty;
            UserName = string.Empty;
            Mark = string.Empty;
            IsAuthenticated = false;
        }
    }
}