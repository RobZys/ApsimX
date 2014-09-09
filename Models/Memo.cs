﻿using System;
using System.Text;
using Models.Core;
using System.Xml.Serialization;
using System.Xml;

namespace Models
{
    /// <summary>
    /// This is a memo/text component that stores user entered text information.
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.HTMLView")]
    [PresenterName("UserInterface.Presenters.MemoPresenter")]
    public class Memo : Model
    {
        public Memo()
        {

        }

        [XmlIgnore]
        [Description("Text of the memo")]
        public string MemoText { get; set; }

        [XmlElement("MemoText")]
        public XmlNode CodeCData
        {
            get
            {
                XmlDocument dummy = new XmlDocument();
                return dummy.CreateCDataSection(MemoText);
            }
            set
            {
                if (value != null)
                {
                    if (value.InnerText.StartsWith("<"))
                        MemoText = value.InnerText;
                    else
                    {
                        
                        MemoText = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                   "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">" +
                                   "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">" +
                                   "<head><title></title></head>" +
                                   "<body><p>" +
                                   value.InnerText +
                                   "</p></body></html>";
                    }
                }
            }
        }
    }
}