using MIQA_xBot;
using System;
using System.Data;
using System.Net;
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Xports
{

    private XportsXporter xporterField;

    /// <remarks/>
    public XportsXporter Xporter
    {
        get
        {
            return this.xporterField;
        }
        set
        {
            this.xporterField = value;
        }
    }

    public DataTable Query(int lagDays)
    {
        // Get the current lagDate
        //DateTime lagDate = DateTime.Today.AddDays(-Settings.lagDays);
        DateTime lagDate = DateTime.Today.AddDays(-lagDays);
        // Replace stuff in the SQLstring
        string sqlQuery = this.Xporter.SQLstring.Replace("this.lastActive", this.Xporter.lastActivity).Replace("less", "<").Replace("todayLag", lagDate.ToString("yyyy-MM-dd")).Trim();
        DataTable plans = SqlInterface.Query(sqlQuery);
        return plans;
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class XportsXporter
{

    private string nameField;

    private bool activeField;

    private string ipstringField;

    private byte portField;

    private string aEtitleField;

    private string sQLstringField;

    private string[] includeField;

    private string lastActivityField;

    private bool allowDoubletsField;

    /// <remarks/>
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public bool active
    {
        get
        {
            return this.activeField;
        }
        set
        {
            this.activeField = value;
        }
    }

    /// <remarks/>
    public string ipstring
    {
        get
        {
            return this.ipstringField;
        }
        set
        {
            this.ipstringField = value;
        }
    }

    /// <remarks/>
    public byte port
    {
        get
        {
            return this.portField;
        }
        set
        {
            this.portField = value;
        }
    }

    /// <remarks/>
    public string AEtitle
    {
        get
        {
            return this.aEtitleField;
        }
        set
        {
            this.aEtitleField = value;
        }
    }

    /// <remarks/>
    public string SQLstring
    {
        get
        {
            return this.sQLstringField;
        }
        set
        {
            this.sQLstringField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("item", IsNullable = false)]
    public string[] include
    {
        get
        {
            return this.includeField;
        }
        set
        {
            this.includeField = value;
        }
    }

    internal string getIP()
    {
        string ip = String.Empty;
        try
        {
            ip = Dns.GetHostAddresses(ipstring)[0].ToString();
        }
        catch
        {
            ip = ipstring;
        }
        return ip;
    }

    /// <remarks/>
    public string lastActivity
    {
        get
        {
            return this.lastActivityField;
        }
        set
        {
            this.lastActivityField = value;
        }
    }

    /// <remarks/>
    public bool allowDoublets
    {
        get
        {
            return this.allowDoubletsField;
        }
        set
        {
            this.allowDoubletsField = value;
        }
    }
}

public class JSONRoot
{
    public Xports Xports { get; set; }
}
