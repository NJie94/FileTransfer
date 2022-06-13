using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace FileTransfer
{
    public class IniHelper
    {
        public const int IND_GROUP = 0;
        public const int IND_PROPERTY = 1;
        public const int IND_DEFAULT_VALUE = 2;

        public string Path;
        public string FileName = "";

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        /// <summary>
        /// IniHelper Constructor
        /// Ini data text file
        /// [Section]
        /// Key=Value
        /// </summary>
        /// <param name="IniFullPath">Ini file path</param>
        /// <param name="fileName">File name</param>
        public IniHelper(string IniFullPath = null, string fileName = "NoName")
        {
            FileName = fileName;
            Path = new FileInfo(IniFullPath != null ? IniFullPath : FileName + ".ini").FullName.ToString();
        }
        /// <summary>
        /// Write configuration KeyValue pair
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key</param>
        /// <param name="Value">Value</param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.Path);
        }
        /// <summary>
        /// Read configuration KeyValue pair
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key</param>
        /// <returns>Value in string</returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            string returnValue;

            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            255, this.Path);
            returnValue = temp.ToString();

            if (returnValue == null || returnValue == "")
                returnValue = "0";

            return returnValue;
        }
        /// <summary>
        /// Read configuration KeyValue pair (Boolean data type)
        /// </summary>
        /// <param name="Section">Section name</param>
        /// <param name="Key">Key name</param>
        /// <returns>Value in string</returns>
        public string IniReadValueBool(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            string returnValue;

            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            255, this.Path);
            returnValue = temp.ToString();

            if (returnValue == null || returnValue == "")
                returnValue = "false";

            return returnValue;
        }
        private string Read(string Key, string Section = null)
        {
            StringBuilder RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section != null ? Section : FileName, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }
        private void Write(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section != null ? Section : FileName, Key, Value, Path);
        }
        /// <summary>
        /// Delete Configuration KeyValue pair
        /// </summary>
        /// <param name="Key">Key name</param>
        /// <param name="Section">Section name</param>
        public void DeleteKey(string Key, string Section = null)
        {
            Write(Key, null, Section != null ? Section : FileName);
        }
        /// <summary>
        /// Delete whole section of KeyValue pairs
        /// </summary>
        /// <param name="Section">section name</param>
        public void DeleteSection(string Section = null)
        {
            Write(null, null, Section != null ? Section : FileName);
        }
        /// <summary>
        /// Check Key exists
        /// </summary>
        /// <param name="propertyInfo">Key properties</param>
        /// <returns></returns>
        public bool KeyExists(string[] propertyInfo)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            return Read(property, group).Length > 0 ? true : false;
        }
        /// <summary>
        /// Delete Key properties
        /// </summary>
        /// <param name="propertyInfo">Key properties</param>
        public void Delete(string[] propertyInfo)
        {
            if (!KeyExists(propertyInfo)) return;

            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            DeleteKey(property, group);
        }
        /// <summary>
        /// Check Key existance
        /// </summary>
        /// <param name="Key">Key name</param>
        /// <param name="Section">Section name</param>
        /// <returns></returns>
        public bool KeyExists(string Key, string Section = null)
        {
            return Read(Key, Section).Length > 0 ? true : false;
        }
        public void SaveSetting(string[] propertyInfo, string value)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            string defaultValue = propertyInfo[IND_DEFAULT_VALUE];

            string writeValue = value == "" ? defaultValue : value;
            Write(property, writeValue, group);
        }
        public void ParseString(string[] propertyInfo, ref string Variable)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            string defaultValue = propertyInfo[IND_DEFAULT_VALUE];
            Variable = "";
            string tempStr = Read(property, group);
            if (tempStr == "")
            {
                SaveSetting(propertyInfo, "");
            }
            else
            {
                Variable = tempStr;
            }
        }
        public void ParseBool(string[] propertyInfo, ref bool variable)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            string defaultValue = propertyInfo[IND_DEFAULT_VALUE];
            variable = false;
            bool tempBool = false;
            string tempStr = Read(property, group);
            if (tempStr == "")
            {
                SaveSetting(propertyInfo, "");
            }
            else
            {
                if (bool.TryParse(tempStr, out tempBool))
                    variable = tempBool;
                else
                    throw new Exception("Fail to parse system info bool <" + group + ", " + property + ", " + tempStr + ">");
            }
        }
        public void ParseInt(string[] propertyInfo,
                              ref int variable, int Min, int Max)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            string defaultValue = propertyInfo[IND_DEFAULT_VALUE];
            string tempStr = Read(property, group);

            variable = 0;
            if (tempStr == "")
            {
                SaveSetting(propertyInfo, "");
            }
            else
            {
                if (!Validator.IsValid(tempStr, Min, Max))
                    throw new Exception("Fail to parse system info integer <" + group + ", " + property + ", " + tempStr + ">");
                else
                    variable = Convert.ToInt32(tempStr);
            }
        }
        public void ParseFloat(string[] propertyInfo,
                              ref float variable, float min, float max)
        {
            string group = propertyInfo[IND_GROUP];
            string property = propertyInfo[IND_PROPERTY];
            string defaultValue = propertyInfo[IND_DEFAULT_VALUE];
            string tempStr = Read(property, group);

            variable = 0;
            if (tempStr == "")
            {
                SaveSetting(propertyInfo, "");
            }
            else
            {
                if (!Validator.IsValid(tempStr, min, max))
                    throw new Exception("Fail to parse system info float <" + group + ", " + property + ", " + tempStr + ">");
                else
                    variable = Convert.ToSingle(tempStr);
            }
        }
    }
}
