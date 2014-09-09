﻿using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace Utility
{
    //=========================================================================
    /// <summary>
    /// This is the configuration settings object.
    /// Add fields here to store extra settings for the application.
    /// </summary>
    [Serializable()]
    public class Settings
    {
        /// <summary>
        /// The location for the form
        /// </summary>
        public Point MainFormLocation;

        /// <summary>
        /// The size of the main form
        /// </summary>
        public Size MainFormSize;

        /// <summary>
        /// The state (max, min, norm) of the form
        /// </summary>
        public System.Windows.Forms.FormWindowState MainFormWindowState;

        /// <summary>
        /// List of the most recently opened files
        /// </summary>
        public List<string> MruList;

        /// <summary>
        /// The maximum number of files allowed in the mru list
        /// </summary>
        private int FilesInHistory; // this could be a user setting

        /// <summary>
        /// The settings class constructor
        /// </summary>
        public Settings()
        {
            MruList = new List<string>();
            FilesInHistory = 10;
        }

        /// <summary>
        /// Add a filename to the list.
        /// </summary>
        /// <param name="filename">File path</param>
        public void AddMruFile(string filename)
        {
            if (filename.Length > 0)
            {
                if (MruList.Count > 0)
                {
                    if (MruList.IndexOf(filename) < 0)
                    {
                        // First time that filename has been added 
                        if (MruList.Count >= FilesInHistory)
                            MruList.RemoveAt(MruList.Count - 1);  // Delete the last item 
                    }
                    else
                    {
                        // Item is in the history list => move to top 
                        MruList.RemoveAt(MruList.IndexOf(filename));
                    }
                    MruList.Insert(0, filename);
                }
                else
                    MruList.Add(filename);
            }
        }

        /// <summary>
        /// Remove a specified file from the list
        /// </summary>
        /// <param name="filename">The file name to delete</param>
        public void DelMruFile(string filename)
        {
            if (filename.Length > 0)
            {
                if (MruList.Count > 0)
                {
                    if (MruList.IndexOf(filename) >= 0)
                    {
                        MruList.RemoveAt(MruList.IndexOf(filename));
                    }
                }
            }
        }

        /// <summary>
        /// Clean the list by removing missing files
        /// </summary>
        public void CleanMruList()
        {
            string filename;
            int i = MruList.Count - 1;
            while (i >= 0)
            {
                filename = MruList[i];
                if (!File.Exists(filename))
                {
                    DelMruFile(filename);
                }
                i--;
            }
        }
    }

    //=========================================================================
    /// <summary>
    /// Handle the reading and writing of the configuration settings file
    /// </summary>
    public class Configuration
    {
        public string Version { get { return "1.0"; } }  //this could be obtained from elsewhere in the application
        public Settings Settings { get; set; }
        private string ConfigurationFile;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Configuration()
        {
            ConfigurationFile = Path.Combine(ConfigurationFolder, "ApsimX.xml");
            //deserialise the file
            if (File.Exists(ConfigurationFile))
            {
                System.Xml.Serialization.XmlSerializer xmlreader = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                StreamReader filereader = new StreamReader(ConfigurationFile);
                Settings = new Settings();
                Settings = (Settings)xmlreader.Deserialize(filereader);
                filereader.Close();
            }
            else
            {
                Settings = new Settings() { MainFormSize = new Size(640, 480), 
                                            MainFormWindowState = System.Windows.Forms.FormWindowState.Maximized };
            }
        }

        /// <summary>
        /// Store the configuration settings to file
        /// </summary>
        public void Save()
        {
            string ConfigPath = Path.GetDirectoryName(ConfigurationFile);
            if (!Directory.Exists(ConfigPath))
                Directory.CreateDirectory(ConfigPath);
            StreamWriter filewriter = new StreamWriter(ConfigurationFile);
            System.Xml.Serialization.XmlSerializer xmlwriter = new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            xmlwriter.Serialize(filewriter, Settings);
            filewriter.Close();
        }

        /// <summary>
        /// Return the configuration folder.
        /// </summary>
        public string ConfigurationFolder
        {
            get
            {
                //On Linux and Mac the path will be .config/
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "ApsimInitiative",
                                    "ApsimX");
            }
        }

        /// <summary>
        /// Return the name of the summary file JPG.
        /// </summary>
        public string SummaryPngFileName
        {
            get
            {
                // Make sure the summary JPG exists in the configuration folder.
                string summaryJpg = Path.Combine(ConfigurationFolder, "ApsimSummary.png");
                if (!File.Exists(summaryJpg))
                {
                    Bitmap b = UserInterface.Properties.Resources.ResourceManager.GetObject("ApsimSummary") as Bitmap;
                    b.Save(summaryJpg);
                }
                return summaryJpg;
            }
        }

    }
}