using EvilDICOM.Network;
using EvilDICOM.Network.DIMSE.IOD;
using EvilDICOM.Network.Querying;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceProcess;

/// <summary>
/// MIQA_xBot is a single purpose implementation of the generalized xBot(https://github.com/SUSradfys/xBot) 
/// MIQA_xBot sends instructions to a ARIA Database Daemon service.
/// The information is used to conduct a DICOM CMOVE operation from the ARIA OIS to MIQA .
/// The EvilDICOM (https://github.com/rexcardan/Evil-DICOM) library is used as a middle hand.
/// </summary>

namespace MIQA_xBot
{
    class Program
    {

        [STAThread]
        static void Main(string[] args)
        {
            // Get MainSettings
            string xml_settings = File.ReadAllText(@"MainSettings.xml");
            MainSettings settings = xml_settings.ParseXML<MainSettings>();

            int xPortedPlans;

            try
            {
                xPortedPlans = Execute(settings);
                if (xPortedPlans > 0)
                {
                    sendMail.Program.send(settings.MAILTO, "MIQA export success", "Number of plans exported: " + xPortedPlans.ToString(), settings.MAIL_USER, settings.MAIL_DOMAIN, settings.SMTP_SERVER);
                }
            }
            catch (Exception exception)
            {
                // send main on failure to execute
                sendMail.Program.send(settings.MAILTO, "MIQA export failure", exception.Message.ToString(), settings.MAIL_USER, settings.MAIL_DOMAIN, settings.SMTP_SERVER);
            }

            
        }


        static int Execute(MainSettings settings)
        {
            // start by sending mail
            sendMail.Program.send(settings.MAILTO, "MIQA_xBot initiated", "Export to MIQA will begin now.", settings.MAIL_USER, settings.MAIL_DOMAIN, settings.SMTP_SERVER);

            // verify that the ARIADBDaemon Windows Service is running
            string serviceName = "vmsdicom_ARIADB";
            ServiceController sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                sc.Start();
                sendMail.Program.send(settings.MAILTO, "MIQA_xBot started " + serviceName, "The service" + serviceName + "was not running, so I started it for you.", settings.MAIL_USER, settings.MAIL_DOMAIN, settings.SMTP_SERVER);
            }

            List<CFindImageIOD> iods = new List<CFindImageIOD>();
            Entity daemon = Entity.CreateLocal(settings.DBDAEMON_AETITLE, settings.DBDAEMON_PORT);

            // define the local service class
            var me = Entity.CreateLocal(settings.SCU_AETITLE, settings.SCU_PORT);
            var scu = new DICOMSCU(me);

            // define the query builder
            var qb = new QueryBuilder(scu, daemon);
            ushort msgId = 1;

            // read the xml file
            string xml = File.ReadAllText(settings.XporterPath);
            Xports xPort = xml.ParseXML<Xports>();

            // define the recievr
            //Entity reciever = new Entity(xPort.Xporter.AEtitle, getIP(xPort.Xporter.ipstring), xPort.Xporter.port);
            Entity reciever = new Entity(xPort.Xporter.AEtitle, xPort.Xporter.getIP(), xPort.Xporter.port);
            DICOMSCP scp = new DICOMSCP(reciever);

            // Query plan
            DataTable plans = new DataTable();
            if (!String.IsNullOrEmpty(xPort.Xporter.SQLstring))
            {
                SqlInterface.Connect(settings);
                plans = xPort.Query(settings.LAG_DAYS);
                SqlInterface.Disconnect();
            }

            // loop through plans
            foreach (DataRow row in plans.Rows)
            {
                var patId = (string)row["PatientId"];
                var planUID = (string)row["UID"];
                iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = planUID });
                // loop through items and query based on type
                foreach (string item in xPort.Xporter.include.ToList())
                {
                    string itemSqlString = string.Empty;
                    switch (item)
                    {
                        case "planDose":
                            itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix where DoseMatrix.PlanSetupSer = " + (Int64)row["PlanSer"];
                            break;
                        case "fieldDoses":
                            itemSqlString = "select distinct UID = DoseMatrix.DoseUID from DoseMatrix, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and DoseMatrix.RadiationSer = Radiation.RadiationSer";
                            break;
                        case "slices":
                            itemSqlString = "select UID=Slice.SliceUID from Slice, Image, StructureSet, PlanSetup where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer and Image.ImageSer=StructureSet.ImageSer and Slice.SeriesSer=Image.SeriesSer";
                            break;
                        case "structures":
                            itemSqlString = "select UID=StructureSet.StructureSetUID from PlanSetup, StructureSet where PlanSetup.PlanSetupSer=" + (Int64)row["PlanSer"] + " and StructureSet.StructureSetSer=PlanSetup.StructureSetSer";
                            break;
                        case "images":
                            itemSqlString = "select UID=Slice.SliceUID from Slice, ImageSlice, Radiation where Radiation.PlanSetupSer = " + (Int64)row["PlanSer"] + " and ImageSlice.ImageSer = Radiation.RefImageSer and Slice.SliceSer = ImageSlice.SliceSer";
                            break;
                        case "records":
                            itemSqlString = "select UID=TreatmentRecord.TreatmentRecordUID from TreatmentRecord, RTPlan where RTPlan.PlanSetupSer= " + (Int64)row["PlanSer"] + " and TreatmentRecord.RTPlanSer=RTPlan.RTPlanSer";
                            break;
                        default:
                            itemSqlString = String.Empty;
                            break;
                    }
                    if (!String.IsNullOrEmpty(itemSqlString))
                    {
                        DataTable includeItem = SqlInterface.Query(itemSqlString);
                        foreach (DataRow itemRow in includeItem.Rows)
                        {
                            iods.Add(new CFindImageIOD() { PatientId = patId, SOPInstanceUID = (string)itemRow["UID"] });
                        }
                    }

                }
            }

            if (xPort.Xporter.active)
            {
                Console.WriteLine(iods.Count.ToString());
                // Remove duplicate UIDs
                if (!xPort.Xporter.allowDoublets)
                    iods = ListHandler.Unique(iods);

                Console.WriteLine(iods.Count.ToString());
                foreach (var iod in iods)
                {
                    // Send it
                    scu.SendCMoveImage(daemon, iod, xPort.Xporter.AEtitle, ref msgId);
                }
            }

            // overwrite lastActivity
            if (plans.Rows.Count > 0)
            {
                // Get last date encountered
                DateTime lastPlan = (DateTime)plans.Rows[plans.Rows.Count - 1]["MAXDate"];
                xPort.Xporter.lastActivity = lastPlan.ToString("yyyy-MM-dd");

                // write xml
                using (FileStream fs = new FileStream(settings.XporterPath, FileMode.Create))
                {
                    XmlSerializer _xSer = new XmlSerializer(typeof(Xports));

                    _xSer.Serialize(fs, xPort);
                }

            }
            if (xPort.Xporter.active)
                return plans.Rows.Count;
            else
                return 0;
        }


    }

}
