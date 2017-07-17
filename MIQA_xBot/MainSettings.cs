using MIQA_xBot;
using System;
using System.Data;

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class MainSettings
{

    private string aRIA_SERVERField;

    private string aRIA_USERNAMEField;

    private string aRIA_PASSWORDField;

    private string aRIA_DATABASEField;

    private string dBDAEMON_AETITLEField;

    private int dBDAEMON_PORTField;

    private string sCU_AETITLEField;

    private int sCU_PORTField;

    private int lAG_DAYSField;

    private string mAILTOField;

    private string mAIL_USERField;

    private string mAIL_DOMAINField;

    private string sMTP_SERVERField;

    private string xporterPathField;

    /// <remarks/>
    public string ARIA_SERVER
    {
        get
        {
            return this.aRIA_SERVERField;
        }
        set
        {
            this.aRIA_SERVERField = value;
        }
    }

    /// <remarks/>
    public string ARIA_USERNAME
    {
        get
        {
            return this.aRIA_USERNAMEField;
        }
        set
        {
            this.aRIA_USERNAMEField = value;
        }
    }

    /// <remarks/>
    public string ARIA_PASSWORD
    {
        get
        {
            return this.aRIA_PASSWORDField;
        }
        set
        {
            this.aRIA_PASSWORDField = value;
        }
    }

    /// <remarks/>
    public string ARIA_DATABASE
    {
        get
        {
            return this.aRIA_DATABASEField;
        }
        set
        {
            this.aRIA_DATABASEField = value;
        }
    }

    /// <remarks/>
    public string DBDAEMON_AETITLE
    {
        get
        {
            return this.dBDAEMON_AETITLEField;
        }
        set
        {
            this.dBDAEMON_AETITLEField = value;
        }
    }

    /// <remarks/>
    public int DBDAEMON_PORT
    {
        get
        {
            return this.dBDAEMON_PORTField;
        }
        set
        {
            this.dBDAEMON_PORTField = value;
        }
    }

    /// <remarks/>
    public string SCU_AETITLE
    {
        get
        {
            return this.sCU_AETITLEField;
        }
        set
        {
            this.sCU_AETITLEField = value;
        }
    }

    /// <remarks/>
    public int SCU_PORT
    {
        get
        {
            return this.sCU_PORTField;
        }
        set
        {
            this.sCU_PORTField = value;
        }
    }

    /// <remarks/>
    public int LAG_DAYS
    {
        get
        {
            return this.lAG_DAYSField;
        }
        set
        {
            this.lAG_DAYSField = value;
        }
    }

    /// <remarks/>
    public string MAILTO
    {
        get
        {
            return this.mAILTOField;
        }
        set
        {
            this.mAILTOField = value;
        }
    }

    /// <remarks/>
    public string MAIL_USER
    {
        get
        {
            return this.mAIL_USERField;
        }
        set
        {
            this.mAIL_USERField = value;
        }
    }

    /// <remarks/>
    public string MAIL_DOMAIN
    {
        get
        {
            return this.mAIL_DOMAINField;
        }
        set
        {
            this.mAIL_DOMAINField = value;
        }
    }

    /// <remarks/>
    public string SMTP_SERVER
    {
        get
        {
            return this.sMTP_SERVERField;
        }
        set
        {
            this.sMTP_SERVERField = value;
        }
    }

    /// <remarks/>
    public string XporterPath
    {
        get
        {
            return this.xporterPathField;
        }
        set
        {
            this.xporterPathField = value;
        }
    }
}

